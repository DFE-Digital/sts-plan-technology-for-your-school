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
            "beautify": "true",
        },
        edge_attr={
            "arrowhead": "vee",
            "arrowsize": "0.5",
        },
    )


def create_questionnaire_flowchart(section: Section) -> Digraph:
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

        for answer in question.answers:
            answer_text = _wrap_text(answer.text, 20)

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
                tree.edge(current_question_id, next_question_id, label=answer_text)
            else:
                tree.edge(current_question_id, "end", label=answer_text)

    return tree


def process_sections(sections: list[Section]) -> None:
    """Generates a graph for each section and saves it to the visualisations folder by section name"""
    png_folder = Path("visualisations")
    png_folder.mkdir(exist_ok=True)

    for section in sections:
        logger.info(f"Generating visualisation for section {section.name}")
        output_file = Path(png_folder, section.name)
        flowchart = create_questionnaire_flowchart(section)
        flowchart.render(output_file, cleanup=True)
