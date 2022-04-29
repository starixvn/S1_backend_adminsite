import React from "react";
import moment from "moment-timezone";
import ActionButtons from "./Components/ActionButton";
import DropDownBox from "./Components/DropDownBox"

const SpecialItemType = {
    actionBtn: "actionBtn",
    statusBtn: "statusBtn"
};

const dataColumnDefs = () => {
    return [
        { dataColumn: "id", columnTitle: "Id" },
        { dataColumn: "gameName", columnTitle: "GameName" },
        { dataColumn: "tpName", columnTitle: "TpName" },
        { dataColumn: "tpGameType", columnTitle: "TpGameType" },
        { dataColumn: "tpCategory", columnTitle: "TpCategory" },
        { dataColumn: "sort", columnTitle: "Sort" },
        {
            dataColumn: "lastUpdateTime",
            columnTitle: "UpdateTime (GMT+8)",
            body: {
                format: dateTimeFormat,
            },
        },
        { dataColumn: "lastUpdateUser", columnTitle: "LastUpdateUser" },
        {
            columnTitle: "Action",
            header: {
                attr: {
                    className: "width-85"
                }
            },
            body: {
                SpecialItem: SpecialItemType.actionBtn
            }
        }
    ];
};

export default function Result(hotGames) {
    const mapOptions = (sortList) => {
        return sortList.map(i => {
            return {
                value: i,
                displayText: i
            };
        });
    };

    const columnDefs = dataColumnDefs();
    let headerRows = [];
    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th
                key={`th-${element.dataColumn}-${idx}`}
                scope="col"
                className={
                    element.header && element.header.attr && element.header.attr.class
                        ? element.header.attr.class
                        : ""}>
                {element.columnTitle}
            </th>);
    });
    let bodyRows = hotGames.Datas.map((data, i) => {
        let row = columnDefs.map((element, j) => {

            let body_content = null;
            if (element.body && element.body.format) {
                let subData = {};
                if (element.body.subData) {
                    element.body.subData.forEach((s) => {
                        subData[s] = data[s];
                    });
                }
                body_content = element.body.format(data[element.dataColumn], subData)
            }
            else if (element.body && element.body.SpecialItem && element.body.SpecialItem === SpecialItemType.actionBtn) {
                body_content = <ActionButtons
                    ClickEdit={hotGames.ClickEdit.bind(this, data["id"])}
                    ClickDelete={hotGames.ClickDelete.bind(this, data["id"])} />
            }
            else if (element.dataColumn === "sort") {

                if (hotGames.isEditSort) {
                    body_content = <DropDownBox
                        options={mapOptions(hotGames.sortList)}
                        dataColumn="tpId"
                        onChange={hotGames.sortListChanged.bind(this, data["id"])}
                        value={data["sort"]} />
                } else {
                    body_content = data["sort"]
                }
            }
            else {
                body_content = data[element.dataColumn]
            }

            return (
                <td
                    key={`td-${element.dataColumn}-${j}`}
                    className={
                        element.body && element.body.attr && element.body.attr.class
                            ? element.body.attr.class
                            : ""
                    }>
                    {body_content}
                </td>
            );
        });

        return <tr key={`result-body-${i}`}>{row}</tr>;
    });

    return (
        <div className="table">
            <table className="table table-striped">
                <thead>
                    <tr>{headerRows}</tr>
                </thead>
                <tbody>{bodyRows}</tbody>
            </table>
        </div>
    );
};

const dateTimeFormat = (dateTime) => {
    return (dateTime === null ? "" : moment(dateTime).format("l LTS"));
};