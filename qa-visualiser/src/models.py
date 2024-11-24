from __future__ import annotations

from typing import TypedDict


class SystemDetails(TypedDict):
    id: str


class Answer(TypedDict):
    sys: SystemDetails
    text: str
    nextQuestion: Question | None
    nextQuestionId: str | None


class Question(TypedDict):
    answers: list[Answer]
    helpText: str | None
    slug: str
    sys: SystemDetails
    text: str


class Section(TypedDict):
    name: str
    questions: list[Question]
    firstQuestionId: str
    interstitialPage: dict | None
    sys: SystemDetails
