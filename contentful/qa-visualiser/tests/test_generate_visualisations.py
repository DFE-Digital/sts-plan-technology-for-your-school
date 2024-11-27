from unittest.mock import patch

from src.generate_visualisations import create_questionnaire_flowchart
from src.models import Section


@patch("src.generate_visualisations.Digraph.node")
@patch("src.generate_visualisations.Digraph.edge")
def test_create_questionnaire_flowchart(mock_edge, mock_node, mock_sections):
    expected_nodes = {
        ("question-1", "First Question?"),
        ("question-2", "Missing Content"),
        ("end", "Check Answers"),
    }
    expected_edges = {
        ("question-1", "question-2"),
        ("question-1", "end"),
    }
    section = Section.model_validate(mock_sections[0])
    create_questionnaire_flowchart(section)

    assert mock_node.call_count == len(expected_nodes)
    assert mock_edge.call_count == len(expected_edges)

    mock_node_args = {call.args for call in mock_node.call_args_list}
    mock_edge_args = {call.args for call in mock_edge.call_args_list}

    assert mock_node_args == expected_nodes
    assert mock_edge_args == expected_edges
