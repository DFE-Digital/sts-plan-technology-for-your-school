from src.fetch_sections import fetch_sections
from src.generate_visualisations import process_sections


def main():
    sections = fetch_sections()
    process_sections(sections)


if __name__ == "__main__":
    main()
