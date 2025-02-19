import logging
import textwrap
from pathlib import Path

from graphviz import Digraph

from src.models import Section

logger = logging.getLogger(__name__)


def _wrap_text(text: str, max_length: int) -> str:
    """Wrap lines such that no line exceeds max_length, splitting on whitespace where possible."""
    return textwrap.fill(
        text,
        width=max_length,
        break_long_words=True,
        break_on_hyphens=True,
    )


def _create_blank_digraph() -> Digraph:
    """Blank graph with styling options"""
    return Digraph(
        format="png",
        graph_attr={
            "rankdir": "LR",
            "nodesep": "0.2",
            "ranksep": "0.5",
            "splines": "true",
        },
        edge_attr={
            "arrowhead": "vee",
            "arrowsize": "0.7",
            "minlen": "2",
        },
    )


def create_questionnaire_flowchart(
    section: Section, recommendation_map: dict[str, str]
) -> Digraph:
    """Create a graph of all possible paths you can take through a section"""
    tree = _create_blank_digraph()

    tree.node(
        "end",
        "Check Answers",
        shape="box",
        style="filled",
        fillcolor="lightblue:white",
        width="2",
        gradient="300",
    )

    for question in section.questions:
        current_question_id = question.sys.id
        question_text = _wrap_text(question.text, 20)

        tree.node(
            current_question_id,
            question_text,
            shape="box",
            style="filled",
            fillcolor="grey:white",
            width="2",
            gradient="200",
        )

        created_recommendation_nodes = {}

        for answer in question.answers:
            answer_text = _wrap_text(answer.text, 20)
            answer_id = answer.sys.id
            answer_node_id = f"ans_{answer_id}"

            tree.node(
                answer_node_id,
                answer_text,
                shape="ellipse",
                style="filled",
                fillcolor="lightgrey:white",
                width="1.5",
            )

            tree.edge(current_question_id, answer_node_id)  # Connect question to answer
            if next_question := answer.next_question:
                next_question_id = next_question.sys.id
                tree.node(
                    next_question_id,
                    "Missing Content",
                    shape="diamond",
                    style="filled",
                    fillcolor="red:white",
                    width="2",
                )

            if answer.next_question:
                next_question_id = answer.next_question.sys.id

                tree.node(
                    next_question_id,
                    "Next Question",
                    shape="diamond",
                    style="filled",
                    fillcolor="blue:white",
                    width="2",
                )
                tree.edge(
                    answer_node_id, next_question_id
                )  # Connect answer to next question
            else:
                tree.edge(answer_node_id, "end")  # Connect answer to end

            # Ensure answer.sys.id exists and is in recommendation_map
            if answer_id in recommendation_map:
                recommendations = recommendation_map[
                    answer_id
                ]  # Get list of recommendations

                for recommendation_text in recommendations:
                    wrapped_text = _wrap_text(recommendation_text, 20)

                    # Check if this recommendation text already has a node
                    if wrapped_text not in created_recommendation_nodes:
                        recommendation_node_id = f"rec_{hash(wrapped_text)}"  # Use hash to create a unique ID
                        tree.node(
                            recommendation_node_id,
                            wrapped_text,
                            shape="note",
                            style="filled",
                            fillcolor="yellow:white",
                            width="2",
                        )
                        created_recommendation_nodes[wrapped_text] = (
                            recommendation_node_id  # Store the node ID
                        )

                    # Connect answer to existing recommendation node
                    tree.edge(
                        answer_node_id,
                        created_recommendation_nodes[wrapped_text],
                        label="",
                        color="red",
                    )

    return tree


def process_sections(
    sections: list[Section], recommendation_map: dict[str, str]
) -> None:
    """Generates a graph for each section and saves it to the visualisations folder by section name"""
    png_folder = Path("visualisations")
    png_folder.mkdir(exist_ok=True)

    for section in sections:
        logger.info(f"Generating visualisation for section {section.name}")
        output_file = Path(png_folder, section.name)
        flowchart = create_questionnaire_flowchart(section, recommendation_map)
        flowchart.render(output_file, cleanup=True)
