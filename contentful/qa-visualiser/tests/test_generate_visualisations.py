from unittest.mock import patch

from src.generate_visualisations import _wrap_text, create_questionnaire_flowchart
from src.models import Section


@patch("src.generate_visualisations.Digraph.node")
@patch("src.generate_visualisations.Digraph.edge")
def test_create_questionnaire_flowchart(mock_edge, mock_node, mock_sections):
    completing_map = {
        "answer-1": ["Keep backups up-to-date"],
        "answer-2": ["Ensure security protocols are followed"],
    }
    inprogress_map = {
        "answer-1": ["Keep backups up-to-date"],
        "answer-2": ["Ensure security protocols are followed"],
    }
    all_recommendations = set()

    wrapped_recommendation_1 = _wrap_text("Keep backups up-to-date", 20)
    wrapped_recommendation_2 = _wrap_text("Ensure security protocols are followed", 20)

    section = Section.model_validate(mock_sections[0])
    create_questionnaire_flowchart(
        section,
        completing_map,
        inprogress_map,
        all_recommendations,
    )

    node_pairs = {(call.args[0], call.args[1]) for call in mock_node.call_args_list if len(call.args) >= 2}

    assert ("question-1", "Q1. First Question?") in node_pairs
    assert ("end", "Check Answers") in node_pairs
    assert ("ans_answer-1", _wrap_text("first answer", 20)) in node_pairs
    assert ("ans_answer-2", _wrap_text("second answer", 20)) in node_pairs

    rec_label_to_id = {
        label: nid
        for (nid, label) in node_pairs
        if label in {wrapped_recommendation_1, wrapped_recommendation_2}
    }
    assert wrapped_recommendation_1 in rec_label_to_id
    assert wrapped_recommendation_2 in rec_label_to_id
    rec_id_1 = rec_label_to_id[wrapped_recommendation_1]
    rec_id_2 = rec_label_to_id[wrapped_recommendation_2]

    edge_calls = [
        (call.args[0], call.args[1], call.kwargs)
        for call in mock_edge.call_args_list
        if call.kwargs.get("style") != "invis"
    ]
    actual_edges = {(src, dst) for (src, dst, _) in edge_calls}

    required_edges = {
        ("question-1", "ans_answer-1"),
        ("question-1", "ans_answer-2"),
        ("ans_answer-1", "question-2"),
        ("ans_answer-2", "end"),
        ("ans_answer-1", rec_id_1),
        ("ans_answer-2", rec_id_2),
    }

    for e in required_edges:
        assert e in actual_edges

    completes_edges = [
        (src, dst, {"label": kwargs.get("label"), "color": kwargs.get("color")})
        for (src, dst, kwargs) in edge_calls
        if kwargs.get("label") == "completes"
    ]
    assert ("ans_answer-1", rec_id_1, {"label": "completes", "color": "green"}) in completes_edges
    assert ("ans_answer-2", rec_id_2, {"label": "completes", "color": "green"}) in completes_edges

    inprogress_edges = [
        (src, dst, {"label": kwargs.get("label"), "color": kwargs.get("color")})
        for (src, dst, kwargs) in edge_calls
        if kwargs.get("label") == "in progress"
    ]
    assert ("ans_answer-1", rec_id_1, {"label": "in progress", "color": "orange"}) in inprogress_edges
    assert ("ans_answer-2", rec_id_2, {"label": "in progress", "color": "orange"}) in inprogress_edges
