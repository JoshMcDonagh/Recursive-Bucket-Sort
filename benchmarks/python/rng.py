import random

def generate_random_array(length, lower, upper):
    array = []
    for i in range(length):
        array.append(random.randint(lower, upper))
    return array

def generate_partially_random_array(length, lower, upper):
    array = generate_random_array(length, lower, upper)
    array.sort()
    num_of_random_vals = int(length / 7)
    for i in range(num_of_random_vals):
        index = random.randint(0, length-1)
        array[index] = random.randint(lower, upper)
    return array