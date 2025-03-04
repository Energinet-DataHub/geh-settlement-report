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


def valid_energy_supplier_ids(supplier_ids: list[str]) -> list[str]:

    # Energy supplier IDs must always consist of 13 or 16 digits
    if any(
        (len(id) != 13 and len(id) != 16) or any(c < "0" or c > "9" for c in id)
        for id in supplier_ids
    ):
        msg = "Energy supplier IDs must consist of 13 or 16 digits"
        raise configargparse.ArgumentTypeError(msg)
    return supplier_ids
