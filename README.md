# DevTools CLI

The `DevTools` CLI provides various commands for managing Amazon Lex bots and AWS Lambda functions.

## General Usage

To get help on any command, use the `-h` or `--help` flag:

```shell
DevTools.exe <COMMAND> -h
```

## Global flag

### `skip check identity`

DevTools prevents executing scripts in production environments by default. If you want to run scripts in a production environment, please use the -s or --skip-check-identity flag.

**Usage**

```shell
    -s, --skip-check-identity
```

## Bots Commands

### `extract`

Extracts Amazon Lex bots from a specified path.

**Usage:**

```shell
DevTools.exe bots extract [OPTIONS]
```

### `set-arns`

Sets the ARNs (Amazon Resource Names) in the bot files.

**Usage:**

```shell
DevTools.exe bots set-arns [OPTIONS]
```

**Options:**

- `-p, --path <PATH>`: The path to the bot files.
- `-n, --arn <ARN>`: The ARN of the Lambda function.

**Example:**

```shell
DevTools.exe bots set-arns -p <PATH_TO_BOTS> -n <LAMBDA_FUNCTION_ARN>
```

### `import`

Imports the bots to Amazon Lex.

**Usage:**

```shell
DevTools.exe bots import [OPTIONS]
```

**Options:**

- `-p, --path <PATH>`: The path to the bot files.

**Example:**

```shell
DevTools.exe bots import -p <PATH_TO_BOTS>
```

### `set-aliases`

Sets aliases for the bots.

**Usage:**

```shell
DevTools.exe bots set-aliases <ALIAS_NAME>
```

**Example:**

```shell
DevTools.exe bots set-aliases <ALIAS_NAME>
```

## Lambda Commands

### `add-bot-permissions`

Adds bot permissions to the Lambda function.

**Usage:**

```shell
DevTools.exe lambda add-bot-permissions [OPTIONS]
```

**Options:**

- `-p, --path <PERMISSIONS_PATH>`: The path to the bot permissions JSON file.
- `-l, --lambda <LAMBDA_FUNCTION_NAME>`: The name of the Lambda function.

**Example:**

```shell
DevTools.exe lambda add-bot-permissions -p <PERMISSIONS_PATH> -l <LAMBDA_FUNCTION_NAME>
```

## Examples

**Import Bots:**

```shell
DevTools.exe bots import -p .\Bots\extracted\zips
```

**Set Aliases for Bots:**

```shell
DevTools.exe bots set-aliases STAGING
```

**Set ARNs for Bots:**

```shell
DevTools.exe bots set-arns -p .\Bots\extracted -n arn:aws:lambda:us-west-2:767397911243:function:stg-lex-lambda-validations
```

**Add Bot Permissions to Lambda:**

```shell
DevTools.exe lambda add-bot-permissions -p  .\bot-permisions.json -l stg-lex-lambda-validations
```
