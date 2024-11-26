from pytest import fixture


@fixture
def mock_answers() -> list[dict]:
    return [
        {
            "sys": {"id": "answer-1"},
            "text": "first answer",
            "nextQuestion": {
                "sys": {"id": "question-2"},
            },
        },
        {
            "sys": {"id": "answer-2"},
            "text": "second answer",
            "nextQuestion": None,
        },
    ]


@fixture
def mock_questions(mock_answers) -> list[dict]:
    return [
        {
            "answers": mock_answers,
            "slug": "question-1-slug",
            "sys": {"id": "question-1"},
            "text": "First Question?",
            "helpText": None,
        }
    ]


@fixture
def mock_sections(mock_questions) -> list[dict]:
    return [
        {
            "name": "Section 1",
            "questions": mock_questions,
            "firstQuestionId": "question-1",
            "interstitialPage": None,
            "sys": {"id": "section-1"},
        }
    ]
