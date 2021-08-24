import React from "react";
import Pagination from "./Pagination";
import moment from "moment";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    {
      dataColumn: "siteName",
      columnTitle: "Site",
    },
    {
      dataColumn: "transId",
      columnTitle: "TransId",
    },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId", "transId"],
      },
    },
    {
      dataColumn: "isoCode",
      columnTitle: "Currency",
    },
    {
      dataColumn: "amount",
      columnTitle: "Amount",
      body: { format: numberFormat },
    },
    {
      dataColumn: "accountingAmount",
      columnTitle: "Accounting Amount",
      body: { format: numberFormat },
    },
    {
      dataColumn: "chargeFee",
      columnTitle: "Charge Fee",
      body: { format: numberFormat },
    },
    {
      dataColumn: "accountingChargeFee",
      columnTitle: "Accounting ChargeFee",
      body: { format: numberFormat },
    },
    {
      dataColumn: "accountingCount",
      columnTitle: "Accounting Count",
    },
    {
      dataColumn: "operator",
      columnTitle: "Operator",
    },
    {
      dataColumn: "flowEndTime",
      columnTitle: "Approved Time" + " (" + moment().format("Z") + ")",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "detail", columnTitle: "Detail" },
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
  let bodyRows = props.Datas.map((data, dataidx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      return element.dataColumn === "detail" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Detail.bind(this, data.transId)}
              disabled={props.DetailBtnDisabled}
            >
              Detail
            </button>
          </div>
        </td>
      ) : (
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
    });

    return <tr key={`result-body-${dataidx}`}>{row}</tr>;
  });
  return (
    <div className="table">
      <div className="table-options">
        <button
          type="button"
          className="btn btn-approved"
          onClick={props.ExportToExcel}
        >
          Export Excel
        </button>
      </div>
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Pagination
        Data={{
          totalPages: props.PageInfo.totalPages,
          pageNumber: props.PageInfo.pageNumber,
          rowCount: props.PageInfo.rowCount,
        }}
        ChangePage={props.ChangePage}
      />
    </div>
  );
};

const userNameFormat = (userName, subData) => {
  return (
    <a
      href={
        "/CustManage/CustInfo?UserName=" +
        userName +
        "&custId=" +
        subData.custId +
        "&TransId=" +
        subData.transId
      }
      target="_blank"
    >
      {userName}
    </a>
  );
};

const numberFormat = (number) => {
  return number_format(number_toFixed(number, 0));
};

const dateFormat = (datetime) => {
  return moment(datetime).format("l LTS");
};

export default Panel;
