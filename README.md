# Wordle Solver

A Wordle solver made in C#. This project is a port from a different
implementation of mine, which was in Java.

The original approach was inspired by
[Implementing and Optimizing a Wordle Solver in Rust](https://youtu.be/doFowk4xj7Q)
by [Jon Gjengset](https://github.com/jonhoo).

> His implementation can be found here: https://github.com/jonhoo/roget

## Installation:

- Clone this repo.
- Run:

```sh
$ dotnet pack -c Release
$ dotnet tool install --global WordleSolver --add-source ./src/WordleSolver.Application/bin/Release/
```

- You can now run with:

```sh
$ Wordle-Solver
```
