import pytest
from pydantic import ValidationError

from src.models import Section


def test_valid_section(mock_sections, mock_questions, mock_answers):
    section = Section.model_validate(mock_sections[0])

    assert section.sys.id == mock_sections[0]["sys"]["id"]
    assert section.name == mock_sections[0]["name"]

    assert len(section.questions) == 1
    question = section.questions[0]
    question_json = mock_questions[0]

    assert question.sys.id == question_json["sys"]["id"]
    assert question.text == question_json["text"]

    assert len(question.answers) == 2

    first_answer = question.answers[0]
    assert first_answer.sys.id == mock_answers[0]["sys"]["id"]
    assert first_answer.text == mock_answers[0]["text"]
    assert first_answer.next_question.model_dump() == mock_answers[0]["nextQuestion"]

    second_answer = question.answers[1]
    assert second_answer.sys.id == mock_answers[1]["sys"]["id"]
    assert second_answer.text == mock_answers[1]["text"]
    assert second_answer.next_question is None


def test_invalid_section(mock_sections):
    invalid_json = mock_sections[0].copy()
    del invalid_json["sys"]["id"]

    with pytest.raises(ValidationError):
        Section.model_validate(invalid_json)
