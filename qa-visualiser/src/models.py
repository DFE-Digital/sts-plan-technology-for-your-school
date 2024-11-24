from __future__ import annotations

from pydantic import BaseModel, ConfigDict
from pydantic.alias_generators import to_camel


class BaseSchema(BaseModel):
    model_config = ConfigDict(alias_generator=to_camel)


class SystemDetails(BaseSchema):
    id: str


class Answer(BaseSchema):
    sys: SystemDetails
    text: str
    next_question: Question | None


class Question(BaseSchema):
    answers: list[Answer]
    help_text: str | None
    sys: SystemDetails
    text: str


class Section(BaseSchema):
    name: str
    questions: list[Question]
    first_question_id: str
    interstitial_page: dict | None
    sys: SystemDetails
