# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from datetime import datetime

import configargparse


def valid_date(s: str) -> datetime:
    """See https://stackoverflow.com/questions/25470844/specify-date-format-for-python-argparse-input-arguments"""
    try:
        return datetime.strptime(s, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError:
        msg = "not a valid date: {0!r}".format(s)
        raise configargparse.ArgumentTypeError(msg)


def valid_energy_supplier_ids(s: str) -> list[str]:
    if not s.startswith("[") or not s.endswith("]"):
        msg = "Energy supplier IDs must be a list enclosed by an opening '[' and a closing ']'"
        raise configargparse.ArgumentTypeError(msg)

    # 1. Remove enclosing list characters 2. Split each id 3. Remove possibly enclosing spaces.
    tokens = [token.strip() for token in s.strip("[]").split(",")]

    # Energy supplier IDs must always consist of 13 or 16 digits
    if any(
        (len(token) != 13 and len(token) != 16)
        or any(c < "0" or c > "9" for c in token)
        for token in tokens
    ):
        msg = "Energy supplier IDs must consist of 13 or 16 digits"
        raise configargparse.ArgumentTypeError(msg)

    return tokens
