import React from "react";
import moment from "moment";
import Popout from "./PicturePopout";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    { dataColumn: "seq", columnTitle: "Seq" },
    {
      dataColumn: "bankAccountNo",
      columnTitle: "Bank AccountNo.",
      body: { format: bankAccountFormat },
    },
    {
      dataColumn: "amount",
      columnTitle: "Amount",
      body: { format: numberFormat },
    },
    {
      dataColumn: "chargeFee",
      columnTitle: "ChargeFee",
      body: { format: numberFormat },
    },
    { dataColumn: "memo", columnTitle: "Memo" },
    { dataColumn: "creatorName", columnTitle: "Creator" },
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime" + " (" + moment().format("Z") + ")",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "operatorName", columnTitle: "Operator" },
    {
      dataColumn: "operateTime",
      columnTitle: "OperateTime" + " (" + moment().format("Z") + ")",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "action", columnTitle: "Action" },
  ];
};
const Panel = (props) => {
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
            : ""
        }
      >
        {element.columnTitle}
      </th>
    );
  });

  let bodyRows = props.AccountingData.map((data, dataidx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      if (element.dataColumn === "action") {
        return (
          <td key={`td-${element.dataColumn}-${idx}`}>
            <div>
              <div className="button-action">
                <button
                  className="btn btn-edit"
                  onClick={props.GetById.bind(this, data.id)}
                >
                  <i className="icon-edit"></i>
                </button>
                <button
                  className="btn btn-approved"
                  type="submit"
                  onClick={props.PopOut_Open.bind(this, data)}
                >
                  Pic
                </button>
                <button
                  className="btn btn-delete"
                  onClick={props.Delete.bind(this, data)}
                >
                  <i className="icon-delete"></i>
                </button>
              </div>
            </div>
          </td>
        );
      } else if (element.dataColumn === "bankAccountNo") {
        return (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
          >
            {element.body.format(
              data.bankCode,
              data.bankAccountName,
              data.bankAccountNo
            )}
          </td>
        );
      } else {
        return (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
          >
            {element.body && element.body.format
              ? element.body.format(data[element.dataColumn], subData)
              : data[element.dataColumn]}
          </td>
        );
      }
    });

    return <tr key={`accounting-body-${dataidx}`}>{row}</tr>;
  });

  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Popout
        data={props.PopOutData}
        handleClose={(e) => props.PopOut_Close()}
        show={props.PopOutStatus}
      />
    </div>
  );
};

const numberFormat = (number) => {
  return number_format(number_toFixed(number, 0));
};

const dateFormat = (datetime) => {
  return moment(datetime).format("l LTS");
};

const bankAccountFormat = (bankCode, bankAccountName, bankAccountNo) => {
  return bankCode + "-" + bankAccountName + "-" + bankAccountNo;
};

export default Panel;
