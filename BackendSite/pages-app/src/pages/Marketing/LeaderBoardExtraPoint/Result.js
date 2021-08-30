import React from "react";
import Pagination from "./Pagination";
import moment from "moment-timezone";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    {
      dataColumn: "activeDate",
      columnTitle: "ActiveDate (GMT+8)",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "siteName", columnTitle: "SiteName" },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId"],
      },
    },
    { dataColumn: "currency", columnTitle: "Currency" },
    { dataColumn: "categoryName", columnTitle: "Category" },
    {
      dataColumn: "extraPoint",
      columnTitle: "ExtraPoint",
      header: {
        attr: {
          class: "text-right",
        },
      },
      body: {
        attr: {
          class: "text-right",
        },
        format: numberFormat,
      },
    },
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    
    {
      dataColumn: "updateTime",
      columnTitle: "UpdateTime (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    { dataColumn: "lastUpdateUser", columnTitle: "LastUpdateUser"},
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
  let bodyRows = props.Datas.map((data, idx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
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
    });

    return <tr key={`result-body-${idx}`}>{row}</tr>;
  });

  return (
    <div className="table">
      <div className="table-options">
      <button
            className="btn btn-add"
            style={{ marginBottom: "20px" }}
            onClick={props.ClickAdd}>
            Add ExtraPoint
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
          currencyAmount: props.PageInfo.currencyAmount
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
        "&TransId="
      }
      target="_blank"
    >
      {userName}
    </a>
  );
};
const numberFormat = (number) => {
  return number_format(number_toFixed(number, 2));
};
const dateTimeFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};
const dateFormat = (Date) => {
  return moment(Date).format("YYYY-MM-DD");
};

export default Panel;
