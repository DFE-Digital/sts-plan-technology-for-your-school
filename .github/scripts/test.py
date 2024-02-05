import json
import os
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

if __name__ == "__main__":
    imagesDir = 'test'
    if not os.path.exists(imagesDir):
        os.makedirs(imagesDir)
        
    json_data_string = '''
    [      
        {
            "QuestionId": "16FlMU8PF9zYVK9Cxqh0TK",
            "QuestionText": "Do you have someone on your senior leadership team who is responsible for technology?",
            "QuestionOrder": "0",
            "Text": "Yes",
            "NextQuestionId": "35iQO4BlGpJdUdpRIeVdwh"
        },
        {
            "QuestionId": "16FlMU8PF9zYVK9Cxqh0TK",
            "QuestionText": "Do you have someone on your senior leadership team who is responsible for technology?",
            "QuestionOrder": "0",
            "Text": "No ",
            "NextQuestionId": "SYxuS728PRAdWmf6XBWkV"
        },
        {
            "QuestionId": "16FlMU8PF9zYVK9Cxqh0TK",
            "QuestionText": "Do you have someone on your senior leadership team who is responsible for technology?",
            "QuestionOrder": "0",
            "Text": "I'm not sure",
            "NextQuestionId": "SYxuS728PRAdWmf6XBWkV"
        },
        {
            "QuestionId": "35iQO4BlGpJdUdpRIeVdwh",
            "QuestionText": "Does the senior leadership team member responsible for technology have clear and documented responsibilities?",
            "QuestionOrder": "1",
            "Text": "I'm not sure",
            "NextQuestionId": "SYxuS728PRAdWmf6XBWkV"
        },
        {
            "QuestionId": "35iQO4BlGpJdUdpRIeVdwh",
            "QuestionText": "Does the senior leadership team member responsible for technology have clear and documented responsibilities?",
            "QuestionOrder": "1",
            "Text": "Yes",
            "NextQuestionId": "SYxuS728PRAdWmf6XBWkV"
        },
        {
            "QuestionId": "35iQO4BlGpJdUdpRIeVdwh",
            "QuestionText": "Does the senior leadership team member responsible for technology have clear and documented responsibilities?",
            "QuestionOrder": "1",
            "Text": "No",
            "NextQuestionId": "SYxuS728PRAdWmf6XBWkV"
        },
        {
            "QuestionId": "SYxuS728PRAdWmf6XBWkV",
            "QuestionText": "Do you have someone on your board of governors who is responsible for technology?",
            "QuestionOrder": "2",
            "Text": "I'm not sure",
            "NextQuestionId": "6hDjVunLQGbGXBNYwKjRzQ"
        },
        {
            "QuestionId": "SYxuS728PRAdWmf6XBWkV",
            "QuestionText": "Do you have someone on your board of governors who is responsible for technology?",
            "QuestionOrder": "2",
            "Text": "No",
            "NextQuestionId": "6hDjVunLQGbGXBNYwKjRzQ"
        },
        {
            "QuestionId": "SYxuS728PRAdWmf6XBWkV",
            "QuestionText": "Do you have someone on your board of governors who is responsible for technology?",
            "QuestionOrder": "2",
            "Text": "Yes",
            "NextQuestionId": "45KSdKdqXu33RDkjlGnGyd"
        },
        {
            "QuestionId": "45KSdKdqXu33RDkjlGnGyd",
            "QuestionText": "Does the governor responsible for technology have clear and documented responsibilities?",
            "QuestionOrder": "3",
            "Text": "I'm not sure",
            "NextQuestionId": "6hDjVunLQGbGXBNYwKjRzQ"
        },
        {
            "QuestionId": "45KSdKdqXu33RDkjlGnGyd",
            "QuestionText": "Does the governor responsible for technology have clear and documented responsibilities?",
            "QuestionOrder": "3",
            "Text": "No",
            "NextQuestionId": "6hDjVunLQGbGXBNYwKjRzQ"
        },
        {
            "QuestionId": "45KSdKdqXu33RDkjlGnGyd",
            "QuestionText": "Does the governor responsible for technology have clear and documented responsibilities?",
            "QuestionOrder": "3",
            "Text": "Yes",
            "NextQuestionId": "6hDjVunLQGbGXBNYwKjRzQ"
        },
        {
            "QuestionId": "6hDjVunLQGbGXBNYwKjRzQ",
            "QuestionText": "Does your school have a documented technology strategy?",
            "QuestionOrder": "4",
            "Text": "Yes",
            "NextQuestionId": "6Sor4547Kl123zlCVPnqaY"
        },
        {
            "QuestionId": "6hDjVunLQGbGXBNYwKjRzQ",
            "QuestionText": "Does your school have a documented technology strategy?",
            "QuestionOrder": "4",
            "Text": "No",
            "NextQuestionId": "2JhulIFwB8VAp44WuueQH2"
        },
        {
            "QuestionId": "6hDjVunLQGbGXBNYwKjRzQ",
            "QuestionText": "Does your school have a documented technology strategy?",
            "QuestionOrder": "4",
            "Text": "I'm not sure",
            "NextQuestionId": "2JhulIFwB8VAp44WuueQH2"
        },
        {
            "QuestionId": "6Sor4547Kl123zlCVPnqaY",
            "QuestionText": "Is your technology strategy part of your overall school improvement plan?",
            "QuestionOrder": "5",
            "Text": "No",
            "NextQuestionId": "2JhulIFwB8VAp44WuueQH2"
        },
        {
            "QuestionId": "6Sor4547Kl123zlCVPnqaY",
            "QuestionText": "Is your technology strategy part of your overall school improvement plan?",
            "QuestionOrder": "5",
            "Text": "Yes",
            "NextQuestionId": "2JhulIFwB8VAp44WuueQH2"
        },
        {
            "QuestionId": "6Sor4547Kl123zlCVPnqaY",
            "QuestionText": "Is your technology strategy part of your overall school improvement plan?",
            "QuestionOrder": "5",
            "Text": "I'm not sure",
            "NextQuestionId": "2JhulIFwB8VAp44WuueQH2"
        },
        {
            "QuestionId": "2JhulIFwB8VAp44WuueQH2",
            "QuestionText": "How often do you review your school's technology needs?",
            "QuestionOrder": "6",
            "Text": "Technology is an agenda point at leadership meetings and the overall strategy is reviewed at least once a year",
            "NextQuestionId": null
        },
        {
            "QuestionId": "2JhulIFwB8VAp44WuueQH2",
            "QuestionText": "How often do you review your school's technology needs?",
            "QuestionOrder": "6",
            "Text": "Technology is occasionally discussed at leadership meetings and the overall strategy is reviewed once a year",
            "NextQuestionId": null
        },
        {
            "QuestionId": "2JhulIFwB8VAp44WuueQH2",
            "QuestionText": "How often do you review your school's technology needs?",
            "QuestionOrder": "6",
            "Text": "There is no formal process for reviewing our technology needs",
            "NextQuestionId": null
        },
        {
            "QuestionId": "2JhulIFwB8VAp44WuueQH2",
            "QuestionText": "How often do you review your school's technology needs?",
            "QuestionOrder": "6",
            "Text": "I'm not sure",
            "NextQuestionId": null
        }
    ]
    ''' 
    question_data = json.loads(json_data_string)
    flowchart = create_questionnaire_flowchart(question_data)
    flowchart.render('test/test', cleanup=True)
