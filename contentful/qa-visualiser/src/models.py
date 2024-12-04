from __future__ import annotations

from pydantic import BaseModel, ConfigDict
from pydantic.alias_generators import to_camel


class BaseSchema(BaseModel):
    """
    Use an alias generator to recieve json with attributes in camelcase
    and convert them to snake_case on serialisation
    """

    model_config = ConfigDict(alias_generator=to_camel)


class SystemDetails(BaseSchema):
    id: str


class QuestionReference(BaseSchema):
    sys: SystemDetails


class Answer(BaseSchema):
    sys: SystemDetails
    text: str
    next_question: QuestionReference | None


class Question(BaseSchema):
    answers: list[Answer]
    sys: SystemDetails
    text: str


class Section(BaseSchema):
    name: str
    questions: list[Question]
    sys: SystemDetails
