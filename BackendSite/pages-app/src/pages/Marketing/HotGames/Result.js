import React from "react";
import moment from "moment-timezone";

const dataColumnDefs = () => {
  return [
    { dataColumn: "id", columnTitle: "Id" },
    { dataColumn: "gameName", columnTitle: "GameName" },
    { dataColumn: "tpName", columnTitle: "TpName" },
    { dataColumn: "tpGameType", columnTitle: "TpGameType" },
    { dataColumn: "tpCategory", columnTitle: "TpCategory" }
  ];
};

export default function Result (hotGames) {
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
                if(element.body.subData) {
                    element.body.subData.forEach((s) => {
                    subData[s] = data[s];
                    });
                }
                body_content = element.body.format(data[element.dataColumn], subData)

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