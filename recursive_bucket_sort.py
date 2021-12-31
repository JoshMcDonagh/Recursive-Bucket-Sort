# Author: Joshua McDonagh

# Checks if the given array is sorted
def is_sorted(array):
    for i in range(1, len(array)):
        if not array[i] >= array[i-1]:
            return False
    return True

# Sorts a given array using buckets recursively
def recursive_bucket_sort(array):
    # Returns array if it contains fewer than two elements
    if len(array) < 2:
        return array
    
    num_of_buckets = len(array)
    buckets = [[] for _ in range(num_of_buckets)]

    # Finds minimum and maximum values in the array
    maximum = None
    minimum = None
    for val in array:
        if maximum == None or val > maximum:
            maximum = val
        if minimum == None or val < minimum:
            minimum = val
    difference = maximum - minimum

    # Places values of the array into buckets
    for val in array:
        index = int((num_of_buckets-1) * ((val - minimum) / difference))
        buckets[index].append(val)
    
    # If any bucket contains more than one element and is unsorted,
    # run the bucket sort on the bucket
    for i in range(0, len(buckets)):
        if len(buckets[i]) > 1 and not is_sorted(buckets[i]):
            buckets[i] = recursive_bucket_sort(buckets[i])

    # Returns the flattened buckets
    return [j for sub in buckets for j in sub]
