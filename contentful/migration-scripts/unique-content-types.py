import json

def get_content_type_count(json_data):
    content_type_count = {}
    total_count = 0

    entries = json_data.get("entries", [])

    for entry in entries:
        content_type = entry.get("sys", {}).get("contentType", {}).get("sys", {}).get("id")
        if content_type:
            if content_type in content_type_count:
                content_type_count[content_type] += 1
            else:
                content_type_count[content_type] = 1
            total_count += 1
    
    return content_type_count, total_count

def main(filename):
    try:
        with open(filename, 'r') as file:
            json_data = json.load(file)

        content_type_count, total_count = get_content_type_count(json_data)

        if content_type_count:
            print("Content Type Counts:")
            for content_type, count in sorted(content_type_count.items()):
                print(f"{content_type}: {count}")
            
            print(f"Total count of all components: {total_count}")
        else:
            print("No content types found in the JSON data.")

    except FileNotFoundError:
        print(f"The '{filename}' file was not found.")
    except json.JSONDecodeError:
        print(f"Error parsing JSON data from the '{filename}' file.")

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 2:
        print("Usage: python unique-content-types.py <filename>")
    else:
        filename = sys.argv[1]
        main(filename)