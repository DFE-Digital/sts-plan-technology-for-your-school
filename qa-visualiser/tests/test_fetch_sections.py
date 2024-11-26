from unittest.mock import patch

import responses
from pytest import fixture
from requests import RequestException

from src.fetch_sections import fetch_sections
from src.models import Section


@fixture(autouse=True)
def mock_environment_variables():
    with patch.dict(
        "os.environ",
        {"PLANTECH_API_KEY": "test-key", "PLANTECH_API_URL": "http://test"},
    ):
        yield


@responses.activate
def test_fetch_sections_success(mock_sections):
    responses.get(
        "http://test/sections",
        json=mock_sections,
        status=200,
    )

    sections = fetch_sections()
    assert isinstance(sections, list)
    assert len(sections) == 1
    assert isinstance(sections[0], Section)
    assert sections[0].name == "Section 1"


@responses.activate
def test_fetch_sections_failure():
    responses.get(
        "http://test/sections",
        body=RequestException("error"),
        status=500,
    )
    sections = fetch_sections()
    assert sections == []
