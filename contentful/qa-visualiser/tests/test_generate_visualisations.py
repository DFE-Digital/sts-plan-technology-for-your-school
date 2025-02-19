from unittest.mock import patch

from src.generate_visualisations import _wrap_text, create_questionnaire_flowchart
from src.models import Section


@patch("src.generate_visualisations.Digraph.node")
@patch("src.generate_visualisations.Digraph.edge")
def test_create_questionnaire_flowchart(mock_edge, mock_node, mock_sections):
    mock_recommendation_map = {
        "answer-1": ["Keep backups up-to-date"],
        "answer-2": ["Ensure security protocols are followed"],
    }

    wrapped_recommendation_1 = _wrap_text("Keep backups up-to-date", 20)
    wrapped_recommendation_2 = _wrap_text("Ensure security protocols are followed", 20)

    rec_id_1 = f"rec_{hash(wrapped_recommendation_1)}"
    rec_id_2 = f"rec_{hash(wrapped_recommendation_2)}"

    expected_nodes = {
        ("question-1", "First Question?"),
        ("end", "Check Answers"),
        ("question-2", "Next Question"),
        ("ans_answer-1", "first answer"),
        ("ans_answer-2", "second answer"),
        (rec_id_1, wrapped_recommendation_1),
        (rec_id_2, wrapped_recommendation_2),
        ("question-2", "Missing Content"),
    }
    expected_edges = {
        ("question-1", "ans_answer-1"),
        ("question-1", "ans_answer-2"),
        ("ans_answer-1", "question-2"),
        ("ans_answer-2", "end"),
        ("ans_answer-1", rec_id_1),
        ("ans_answer-2", rec_id_2),
    }

    section = Section.model_validate(mock_sections[0])
    create_questionnaire_flowchart(section, mock_recommendation_map)

    assert mock_node.call_count == len(expected_nodes)
    assert mock_edge.call_count == len(expected_edges)

    mock_node_args = {call.args for call in mock_node.call_args_list}
    mock_edge_args = {call.args for call in mock_edge.call_args_list}

    assert mock_node_args == expected_nodes
    assert mock_edge_args == expected_edges
