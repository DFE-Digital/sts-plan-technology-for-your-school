import os
import pyodbc
import json

def query_to_json(connection_string, sql_query):
    connection = pyodbc.connect(connection_string)
    cursor = connection.cursor()
    cursor.execute(sql_query)
    result = cursor.fetchall()
    connection.close()
    return cursor, result

def main():

    connection_string = os.getenv("SQL_CONNECTION_STRING")

    data_folder = 'data'
    if not os.path.exists(data_folder):
        os.makedirs(data_folder)

    section_query = "SELECT id, name FROM [Contentful].[Sections]"
    cursor, sections = query_to_json(connection_string, section_query)

    for section in sections:
        section_id, section_name = section

        sql_query = f'''
            SELECT
                Q.Id AS QuestionId,
                Q.Text AS QuestionText,
                O.[Order] AS QuestionOrder,
                A.Text,
                A.NextQuestionId
            FROM
                [Contentful].[Questions] Q
            JOIN
                [Contentful].[ContentComponents] O ON Q.Id = O.Id
            LEFT JOIN
                [Contentful].[Answers] A ON Q.Id = A.ParentQuestionId
            WHERE
                Q.SectionId = '{section_id}'
            ORDER BY
                O.[Order] ASC;
        '''

        cursor, result = query_to_json(connection_string, sql_query)

        json_result = json.dumps([dict(zip([column[0] for column in cursor.description], row)) for row in result], indent=2)

        output_file = os.path.join(data_folder, f'{section_name}.json')
        with open(output_file, 'w') as file:
            file.write(json_result)

if __name__ == "__main__":
    main()
