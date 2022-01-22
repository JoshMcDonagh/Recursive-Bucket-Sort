terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
}

provider "aws" {
  region = "eu-west-1"
}

##
## Networking
##

resource "aws_vpc" "main" {
  cidr_block = "10.0.0.0/16"
  enable_dns_hostnames = true
  enable_dns_support = true
  tags = {
    Name = "main"
  }
}

resource "aws_subnet" "subnets" {
  for_each = {
    pub1 : { cidr : "10.0.1.0/24", az : "eu-west-1a" },
    pub2 : { cidr : "10.0.2.0/24", az : "eu-west-1b" },
    pub3 : { cidr : "10.0.3.0/24", az : "eu-west-1c" }
  }

  availability_zone       = each.value.az
  cidr_block              = each.value.cidr
  map_public_ip_on_launch = true
  vpc_id                  = aws_vpc.main.id
  tags = {
    Name = each.key
  }
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.main.id
  tags = {
    Name = "main-igw"
  }
}

resource "aws_main_route_table_association" "rta" {
  route_table_id = aws_route_table.rt.id
  vpc_id         = aws_vpc.main.id
}

resource "aws_route_table" "rt" {
  vpc_id = aws_vpc.main.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }
}

resource "aws_security_group" "rds" {
  name   = "rds"
  vpc_id = aws_vpc.main.id

  ingress {
    cidr_blocks = ["0.0.0.0/0"]
    from_port   = 5432
    to_port     = 5432
    protocol    = "TCP"
  }

  egress {
    cidr_blocks = ["0.0.0.0/0"]
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
  }
}

resource "aws_security_group" "instance" {
  name   = "instance"
  vpc_id = aws_vpc.main.id

  ingress {
    cidr_blocks = ["0.0.0.0/0"]
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
  }

  egress {
    cidr_blocks = ["0.0.0.0/0"]
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
  }
}

##
## EC2
##

resource "aws_instance" "server" {
  ami                    = "ami-01efa4023f0f3a042"
  instance_type          = "t2.micro"
  iam_instance_profile   = aws_iam_instance_profile.ssm.name
  vpc_security_group_ids = [aws_security_group.instance.id]
  subnet_id              = aws_subnet.subnets["pub1"].id

  tags = {
    Name = "algorithms-server"
  }
}

output "server_ip" {
  value = aws_instance.server.public_ip
}

##
## IAM
##

resource "aws_iam_role" "ssm" {
  name = "instance-ssm"
  assume_role_policy = jsonencode({
    "Version" : "2012-10-17",
    "Statement" : [
      {
        "Effect" : "Allow",
        "Principal" : { "Service" : "ec2.amazonaws.com" },
        "Action" : "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ssm" {
  role       = aws_iam_role.ssm.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_iam_instance_profile" "ssm" {
  name = "instance-ssm"
  role = aws_iam_role.ssm.name
}

##
## RDS
##

resource "random_password" "rds_password" {
  length  = 16
  special = false
}

resource "aws_db_instance" "postgres" {
  allocated_storage      = 20
  engine                 = "postgres"
  engine_version         = "12.7"
  instance_class         = "db.t2.micro"
  name                   = "main"
  username               = "postgres"
  password               = random_password.rds_password.result
  multi_az               = true
  db_subnet_group_name   = aws_db_subnet_group.rds_subnet_group.id
  vpc_security_group_ids = [aws_security_group.rds.id]
  publicly_accessible = true

  tags = {
    Name = "main"
  }
}

resource "aws_db_subnet_group" "rds_subnet_group" {
  name       = "psql-subnet-group"
  subnet_ids = [for subnet in aws_subnet.subnets : subnet.id]
}

output "db_password" {
  value     = random_password.rds_password.result
  sensitive = true
}

##
## MISC
##

resource "aws_budgets_budget" "because_aws_is_shit" {
  name         = "dont-go-over-15-dollars-monthly"
  budget_type  = "COST"
  limit_amount = "15"
  limit_unit   = "USD"
  time_unit    = "MONTHLY"

  notification {
    comparison_operator        = "GREATER_THAN"
    threshold                  = 100
    threshold_type             = "PERCENTAGE"
    notification_type          = "ACTUAL"
    subscriber_email_addresses = ["joshy@example.com"]
  }
}

resource "aws_s3_bucket" "ssm" {
  bucket = "aws-ssm-joshy"
}

resource "local_file" "ansible_inventory" {
  content  = <<-TEMPLATE
    [server]
    ${aws_instance.server.id} ansible_aws_ssm_region=eu-west-1 ansible_connection=aws_ssm ansible_ssm_region=eu-west-1 ansible_aws_ssm_bucket_name=${aws_s3_bucket.ssm.bucket}
    [local]
    127.0.0.1 ansible_connection=local
  TEMPLATE
  filename = "hosts.ini"
}

resource "local_file" "ansible_params" {
  content  = <<-TEMPLATE
    db_host: ${aws_db_instance.postgres.address}
    db_port: ${aws_db_instance.postgres.port}
    db_user: joshy
    db_pass: woshy
    db_db: algorithms
  TEMPLATE
  filename = "params.example.yml"
}
