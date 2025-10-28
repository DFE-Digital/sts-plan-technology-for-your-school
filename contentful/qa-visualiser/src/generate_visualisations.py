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
    section: Section,
    completing_map: dict[str, list[str]],
    inprogress_map: dict[str, list[str]],
    all_recommendations: set[str],  # kept for compatibility; not used here
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

    for index, question in enumerate(section.questions):
        current_question_id = question.sys.id
        question_text = _wrap_text(question.text, 20)

        # question node
        tree.node(
            current_question_id,
            f"Q{index+1}. {question_text}",
            shape="box",
            style="filled",
            fillcolor="grey:white",
            width="2",
            gradient="200",
        )

        # Deduplicate recommendation nodes *within this question only*
        # so they render close to this question instead of a global cluster.
        question_rec_nodes: dict[str, str] = {}

        for answer in question.answers:
            answer_text = _wrap_text(answer.text, 20)
            answer_id = answer.sys.id
            answer_node_id = f"ans_{answer_id}"

            # answer node
            tree.node(
                answer_node_id,
                answer_text,
                shape="ellipse",
                style="filled",
                fillcolor="lightgrey:white",
                width="1.5",
            )

            tree.edge(current_question_id, answer_node_id)  # Connect question to answer

            if answer.next_question:
                next_question_id = answer.next_question.sys.id
                # do not predefine the node - just connect to it. actual question node will be defined later
                tree.edge(answer_node_id, next_question_id)
            else:
                tree.edge(answer_node_id, "end")

            # helper method to get/create a recommendation node local to this question
            def get_question_rec_node(rec_text: str) -> str:
                wrapped = _wrap_text(rec_text, 20)
                if wrapped not in question_rec_nodes:
                    rec_node_id = f"rec_{hash((current_question_id, wrapped))}"
                    tree.node(
                        rec_node_id,
                        wrapped,
                        shape="note",
                        style="filled",
                        fillcolor="yellow:white",
                        width="2",
                    )
                    question_rec_nodes[wrapped] = rec_node_id

                    # invisible edge to keep the rec near this question
                    tree.edge(current_question_id, rec_node_id, style="invis")
                return question_rec_nodes[wrapped]

            # completed recommendations status - green
            for rec_text in completing_map.get(answer_id) or []:
                rec_node_id = get_question_rec_node(rec_text)
                tree.edge(answer_node_id, rec_node_id, label="completes", color="green")

            # in progress recommendation status - orange
            for rec_text in inprogress_map.get(answer_id) or []:
                rec_node_id = get_question_rec_node(rec_text)
                tree.edge(
                    answer_node_id, rec_node_id, label="in progress", color="orange"
                )

    return tree


def process_sections(
    sections: list[Section],
    completing_map: dict[str, list[str]],
    inprogress_map: dict[str, list[str]],
    all_recommendations: set[str],
) -> None:
    """Generates a graph for each section and saves it to the visualisations folder by section name"""
    png_folder = Path("visualisations")
    png_folder.mkdir(exist_ok=True)

    if not sections:
        logger.warning("No sections found. Nothing to visualise.")
        return

    for section in sections:
        logger.info(f"Generating visualisation for section {section.name}")
        output_file = Path(png_folder, section.name)
        flowchart = create_questionnaire_flowchart(
            section=section,
            completing_map=completing_map,
            inprogress_map=inprogress_map,
            all_recommendations=all_recommendations,
        )
        flowchart.render(output_file, cleanup=True)
