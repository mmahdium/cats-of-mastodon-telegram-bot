# Disclaimer:

This project involves scraping public data from Mastodon timelines. While this data is generally publicly accessible, it's important to note that scraping and archiving such data may have implications, especially if it involves sensitive information or terms of service violations. 

I am not responsible for any misuse or unauthorized use of the scraped data. It is the user's responsibility to ensure compliance with all applicable laws, regulations, and terms of service when storing, archiving, or using the data.

## Required Environment Variables

| Variable Name | Description | Default Value | Format |
|---|---|---|---|
| DB_NAME | Database file name | (Required) | Must not have any extension, plain text |
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

1. Download the published executable for `Linux-x86` from the [pipeline artifacts](https://gitlab.com/mahdium/cats-of-mastodon-telegram-bot/-/pipelines/latest)
2. Navigate to the directory containing the downloaded archive in your terminal and extract the archive.

**Providing Environment Variables:**

**Using a `.env` file:**
1. Create a file named .env in the root directory of your project.
2. Add each environment variable on a separate line in the format KEY=VALUE. For example:
```
DB_NAME=my_data
BOT_TOKEN=your_telegram_bot_token
TAG=mastodontimelinetag
CHANNEL_NUMID=1234567890
ADMIN_NUMID=9876543210
```
1. Run the following command:
```bash
dotnet run
or
./mstdnCats 
```
**Remember to replace `your_telegram_bot_token`, `my_data.json`, `important_data`, `1234567890`, and `9876543210` with your actual values.**