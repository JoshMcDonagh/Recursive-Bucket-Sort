# Overview

This is the terraform code for the service's infrastructure.

# Cost

All resources should be within the AWS free tier... I think.

Things like bandwith have their own 'free usage' quota before you start paying for it. This project shouldn't even be close to hitting it.

After the free tier is over, this should cost around £20/month

# Security

The security is definitely not great (very permissive security groups; a publicially accessable RDS; using AWS-owned policies etc.) however for this particular project it shouldn't be too bad an issue, since this data isn't sensitive or critical.