from abc import abstractmethod
import os
from typing import List
import json
import platform
import timsort
import quicksort
import recursive_bucket_sort
import timeit
from rng import generate_partially_random_array, generate_random_array


class SortMethod:
    @abstractmethod
    def sort(self, array: List[int]) -> List[int]:
        pass


class TimSort(SortMethod):
    def sort(self, array: List[int]) -> List[int]:
        timsort.timSort(array)
        return array


class QuickSort(SortMethod):
    def sort(self, array: List[int]) -> List[int]:
        quicksort.quickSort(array, 0, len(array) - 1)
        return array


class RecursiveBucketSort(SortMethod):
    def sort(self, array: List[int]) -> List[int]:
        return recursive_bucket_sort.recursive_bucket_sort(array)


sorts = [TimSort(), QuickSort(), RecursiveBucketSort()]

array_sizes = [
    100,
    1200,
    2300,
    3400,
    4500,
    5600,
    6700,
    7800,
    8900,
    10000,
    11100,
    12200,
    13300,
    14400,
    15500,
    16600,
    17700,
    18800,
    19900,
    21000,
    22100,
    23200,
    24300,
    25400,
    26500,
    27600,
    28700,
    29800,
    30900,
    32000,
    33100,
    34200,
    35300,
    36400,
    37500,
    38600,
    39700,
    40800,
    41900,
    43000,
    44100,
    45200,
    46300,
    47400,
    48500,
    49600,
    50000,
    55000,
    60000,
    65000,
    70000,
    75000,
    80000,
    85000,
    90000,
    95000,
    100000
]

recurrences = 1

csv = []

for sort in sorts:
    for size in array_sizes:
        for recurrence in range(recurrences):
            print(sort, size, recurrence)

            array = generate_random_array(size, 0, 4000000000)
            fully_random_time = timeit.timeit(lambda: sort.sort(array), number=1)

            array = generate_partially_random_array(size, 0, 4000000000)
            partially_random_time = timeit.timeit(lambda: sort.sort(array), number=1)

            csv.append(
                ",".join(
                    str(value)
                    for value in [
                        type(sort).__name__,
                        size,
                        recurrence,
                        fully_random_time,
                        partially_random_time,
                    ]
                )
            )

output = {
    "results": "\n".join([s for s in csv]),
    "cpu": platform.processor(),
    "platform": platform.platform(),
    "arch": platform.architecture(),
    "python": platform.python_implementation(),
    "python_version": platform.python_version(),
    "recurrences": recurrences,
}

with open(
    "results.json" if "OUTPUT" not in os.environ else os.environ["OUTPUT"], mode="w"
) as f:
    f.write(json.dumps(output))
