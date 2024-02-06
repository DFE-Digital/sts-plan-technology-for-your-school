import os
import json
from graphviz import Digraph

def create_questionnaire_flowchart(question_data):
    tree = Digraph(comment='Questionnaire Flowchart', format='png')

    end_nodes = set()

    for item in question_data:
        current_question = item["QuestionId"]
        next_question_id = item["NextQuestionId"]
        answer_text = item["Text"]

        tree.node(current_question, item["QuestionText"], shape='box')

        if next_question_id:
            tree.node(next_question_id, "", shape='box')
            tree.edge(current_question, next_question_id, label=answer_text)
        else:
            end_nodes.add(current_question)

    for end_node in end_nodes:
        tree.node("end", "End", shape='box')
        tree.edge(end_node, "end", label="No More Questions")

    return tree

def process_data_files():
    data_folder = 'data'
    png_folder = 'visualisations'
    if not os.path.exists(png_folder):
        os.makedirs(png_folder)

    for filename in os.listdir(data_folder):
        if filename.endswith('.json'):
            file_path = os.path.join(data_folder, filename)
            with open(file_path, 'r') as file:
                json_data = json.load(file)

            flowchart = create_questionnaire_flowchart(json_data)
            output_file = os.path.join(png_folder, f'{os.path.splitext(filename)[0]}')
            flowchart.render(output_file, cleanup=True)

if __name__ == '__main__':
    process_data_files()
