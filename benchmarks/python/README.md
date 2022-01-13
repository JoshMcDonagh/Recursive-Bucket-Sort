# Overview

This is the python benchmark, comparing TimSort, QuickSort, and 'Recursive-Bucket-Sort' against one another.

Just to make it clear, the TimSort and QuickSort implementations were made by the GeeksForGeeks team. The only modifications
were removing their 'main' functions.

# Tools required

* Docker
* make

# Running

If you want to run the CPython test, run `make cpython` and then a `cpython.json` will be created.

If you want to run the Pypy test, run `make pypy` and then a `pypy.json` will be created.

If you want to run both, run `make both`.

# Results

The resulting JSON documents are in this format:

```json
{
    "results": "<embedded CSV file, see below for format>",
    "cpu": "<model of the cpu of the host>",
    "platform": "<host platform>",
    "arch": "<host cpu architecture>",
    "python": "<python implementation name>",
    "python_version": "<python version being used>",
    "recurrences": number:<how many times each array size was used in this test>,
}
```

The CSV file is in this format:

```csv
SortName,ArraySize,Recurrence,FullyRandomTimeInSecondsAsFloat,PartiallyRandomTimeInSecondsAsFloat
```

Note that the two time values may be stored in scientific notation.