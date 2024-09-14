# Disclaimer:

This project involves scraping public data from Mastodon timelines. While this data is generally publicly accessible, it's important to note that scraping and archiving such data may have implications, especially if it involves sensitive information or terms of service violations. 

I am not responsible for any misuse or unauthorized use of the scraped data. It is the user's responsibility to ensure compliance with all applicable laws, regulations, and terms of service when storing, archiving, or using the data.

## Required Environment Variables

| Variable Name | Description | Default Value | Format |
|---|---|---|---|
| DB_NAME | Database file name | (Required) | Must end in *.json |
| BOT_TOKEN | Telegram bot token | (Required) | Standard Telegram bot token format |
| TAG | Mastodon timeline tag | (Required) | Text with no spaces |
| CHANNEL_NUMID | Telegram channel number ID | (Required) | Telegram channel number ID format |
| ADMIN_NUMID | Telegram bot admin/reviewer number ID | (Required) | Telegram user number ID format |

## Optional Environment Variables

| Variable Name | Description | Default Value | Format |
|---|---|---|---|
| CUSTOM_INSTANCE | Custom Mastodon instance URL | [https://haminoa.net](https://haminoa.net) | URL format |

**Note:** All environment variables except `CUSTOM_INSTANCE` are required for the project to function.

## Running the Project

### Published Executable

1. Download the published executable for your operating system from the project releases.
2. Navigate to the directory containing the downloaded executable in your terminal.

**Providing Environment Variables:**

* **Windows:**
    - Open a command prompt and navigate to the directory containing the executable.
    - Set environment variables before running the program using the `set` command. For example:
    ```bash
    set DB_NAME=my_data.json BOT_TOKEN=your_telegram_bot_token TAG=important_data CHANNEL_NUMID=1234567890 ADMIN_NUMID=9876543210
    your_project_name.exe
    ```
* **macOS/Linux:**
    - Open a terminal and navigate to the directory containing the executable.
    - Set environment variables before running the program using the `export` command. For example:
    ```bash
    export DB_NAME=my_data.json BOT_TOKEN=your_telegram_bot_token TAG=important_data CHANNEL_NUMID=1234567890 ADMIN_NUMID=9876543210
    ./your_project_name
    ```

**Replace `your_project_name` with the actual name of the downloaded executable file and your own environment variable values.**

### Using `dotnet run`

1. Ensure you have .NET 8 installed on your system.
2. Open a terminal and navigate to the root directory of your project where the `.csproj` file resides.

**Providing Environment Variables:**

There are two ways to provide environment variables when using `dotnet run`:

**A. Using the `DOTNET_VARIABLES` environment variable:**

   Set the `DOTNET_VARIABLES` environment variable to a semicolon-separated list of key-value pairs for your environment variables. For example:

   ```bash
   export DOTNET_VARIABLES="DB_NAME=my_data.json;BOT_TOKEN=your_telegram_bot_token;TAG=important_data;CHANNEL_NUMID=1234567890;ADMIN_NUMID=9876543210"
   dotnet run
   ```
**B. Using a `.env` file:**
1. Create a file named .env in the root directory of your project.
2. Add each environment variable on a separate line in the format KEY=VALUE. For example:
```
DB_NAME=my_data.json
BOT_TOKEN=your_telegram_bot_token
TAG=important_data
CHANNEL_NUMID=1234567890
ADMIN_NUMID=9876543210
```
3. Run the following command:
```bash
dotnet run
```
**Remember to replace `your_telegram_bot_token`, `my_data.json`, `important_data`, `1234567890`, and `9876543210` with your actual values.**