#!/usr/bin/env python3
"""Smoke-test real MVC registration and login for both RaceDay roles.

Requires API listening at 5081 and MVC website listening at 5080.
Runs through HTML forms with ASP.NET anti-forgery tokens and cookie sessions.
Creates uniquely named test accounts in the local database. Never use in prod.
"""
from __future__ import annotations
from html.parser import HTMLParser
from http.cookiejar import CookieJar
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import HTTPCookieProcessor, Request, build_opener
import sys
import secrets
import re
from html import unescape

BASE = 'http://localhost:5080'

class FormParser(HTMLParser):
    def __init__(self):
        super().__init__()
        self.token = None

    def handle_starttag(self, tag, attrs):
        if tag != 'input':
            return
        attrs = dict(attrs)
        if attrs.get('name') == '__RequestVerificationToken':
            self.token = attrs.get('value')

def get_token(opener, page):
    with opener.open(BASE + page, timeout=20) as response:
        markup = response.read().decode('utf-8', 'replace')
        if response.status != 200:
            raise RuntimeError(f'{page} returned HTTP {response.status}')
    parser = FormParser()
    parser.feed(markup)
    if not parser.token:
        raise RuntimeError(f'{page} has no ASP.NET antiforgery token')
    return parser.token

def submit(opener, page, fields):
    encoded = urlencode(fields).encode('utf-8')
    request = Request(BASE + page, data=encoded,
                      headers={'Content-Type': 'application/x-www-form-urlencoded'},
                      method='POST')
    with opener.open(request, timeout=30) as response:
        text = response.read().decode('utf-8', 'replace')
        return response.geturl(), response.status, text

def extract_validation_errors(markup):
    """Show messages from Bootstrap validation spans/divs, not just page HTML header."""
    snippets = re.findall(
        r'<(?:div|span)[^>]*class=["\'][^"\']*(?:text-danger|validation-summary-errors)[^"\']*["\'][^>]*>(.*?)</(?:div|span)>',
        markup, flags=re.IGNORECASE | re.DOTALL)
    messages = []
    for snippet in snippets:
        message = unescape(re.sub(r'<[^>]+>', ' ', snippet))
        message = ' '.join(message.split())
        if message and message not in messages:
            messages.append(message)
    return messages

def test_role(role):
    opener = build_opener(HTTPCookieProcessor(CookieJar()))
    email = 'raceday-test-' + secrets.token_hex(6) + '@example.com'
    password = 'RDay!1' + secrets.token_hex(12) + 'Z'
    registration_token = get_token(opener, '/Account/Register')
    url, status, text = submit(opener, '/Account/Register', {
        '__RequestVerificationToken': registration_token,
        'FirstName': 'Test', 'LastName': role,
        'Email': email, 'Password': password,
        'ConfirmPassword': password, 'Role': role,
    })
    if '/Account/Login' not in url:
        raise RuntimeError(f'{role} registration did not redirect to login; '
                           f'HTTP {status}; validation messages: {extract_validation_errors(text)!r}; '
                           f'page excerpt: {text[-800:]!r}')
    print(f'PASS: {role} registration (redirected to login)')
    login_token = get_token(opener, '/Account/Login')
    url, status, text = submit(opener, '/Account/Login', {
        '__RequestVerificationToken': login_token,
        'Email': email, 'Password': password,
    })
    expected = '/Dashboard' if role == 'Organiser' else '/Events/Explore'
    if expected not in url or status != 200:
        raise RuntimeError(f'{role} login did not reach {expected}; '
                           f'got {url} (HTTP {status}); page excerpt: {text[:450]!r}')
    if 'Sign out' not in text:
        raise RuntimeError(f'{role} dashboard did not show a logged-in session')
    print(f'PASS: {role} login, API session forwarding and role-based page at {expected}')
    with opener.open(BASE + '/Profile/Index', timeout=20) as response:
        if response.status != 200 or '/Account/Login' in response.geturl():
            raise RuntimeError(f'{role} profile was not accessible after login')
    print(f'PASS: {role} can load profile')

if __name__ == '__main__':
    print('RaceDay Part 3 MVC smoke test')
    try:
        for role in ('Participant', 'Organiser'):
            test_role(role)
    except (HTTPError, URLError, RuntimeError) as error:
        print(f'FAIL: {error}', file=sys.stderr)
        sys.exit(1)
    print('All registration, login and role-navigation smoke tests passed.')
