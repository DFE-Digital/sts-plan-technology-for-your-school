import os
import json
from graphviz import Digraph

def split_text(text, max_length):
    if len(text) <= max_length:
        return [text]
    else:
        split_index = text.rfind(' ', 0, max_length)
        if split_index == -1:
            return [text[:max_length]] + split_text(text[max_length:], max_length)
        else:
            return [text[:split_index]] + split_text(text[split_index+1:], max_length)

def create_questionnaire_flowchart(question_data):
    tree = Digraph(format='png')

    end_nodes = set()

    for item in question_data:
        current_question = item["QuestionId"]
        next_question_id = item["NextQuestionId"]
        answer_text = item["Text"]

        question_lines = split_text(item["QuestionText"], 20)
        question_text = "\n".join(question_lines)

        tree.node(current_question, question_text, shape='box', style='filled', fillcolor='grey:white', width='2', gradient='200')

        answer_lines = split_text(answer_text, 20)
        answer_text = "\n".join(answer_lines)

        if next_question_id:
            tree.node(next_question_id, "Missing Content", shape='diamond', style='filled', fillcolor='red:white', width='2')
            tree.edge(current_question, next_question_id, label=answer_text, color='black')
        else:
            end_nodes.add(current_question)

    for end_node in end_nodes:
        tree.node("end", "Check Answers", shape='box', style='filled', fillcolor='lightblue:white', width='2', gradient='300')
        tree.edge(end_node, "end", label="No More Questions", color='black')

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
            flowchart.graph_attr['rankdir'] = 'LR'
            flowchart.graph_attr['beautify'] = 'true'
            flowchart.edge_attr['arrowhead'] = 'vee'
            flowchart.edge_attr['arrowsize'] = '0.5'

            flowchart.render(output_file, cleanup=True)

if __name__ == '__main__':
    process_data_files()
