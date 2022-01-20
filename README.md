# Recursive Bucket Sorting Algorithm
An algorithm (implemented here in Python) mainly inspired by the Bucket Sort (and the Pigeonhole Sort) and works by placing the values of a given unsorted array into buckets where the number of buckets is equal to the length of the unsorted array. In addition, it recursively runs the sorting algorithm on any buckets that contain more than one element and is unsorted.

For more, including why the algorithm has been implemented in this way and how it compares against other sorting algorithms, please read my blog posts here: https://www.joshuamcdonagh.com/search/label/Recursive%20Bucket%20Sort

# Benchmarks

Currently there is only a benchmark implemented in python. Please see the relevant [folder](./benchmarks/python/) for instructions
on how to run the benchmark.
