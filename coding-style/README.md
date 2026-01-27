# Coding Style Guide

## Requirements

### CSharpier

```bash
dotnet tool install csharpier
```

### Python

Available from the [Python 3.14.2 release page](https://www.python.org/downloads/release/python-3142/)
or simply click this link to download directly: [https://www.python.org/ftp/python/3.14.2/python-3.14.2-amd64.exe](https://www.python.org/ftp/python/3.14.2/python-3.14.2-amd64.exe)

### Pre-commit

To install hooks (one-time):

```bash
py -m pip install --user pre-commit
dotnet tool restore
```

### Markdown formatter

- DavidAnson.vscode-markdownlint

## Running against all files

If you're utterly mad and want to do a full-repo format, you can do this:

```bash
py -m pre_commit run --all-files
```

## Documentation

- [Formatting](./FORMATTING.md)
- [TOOLING](./TOOLING.md)
