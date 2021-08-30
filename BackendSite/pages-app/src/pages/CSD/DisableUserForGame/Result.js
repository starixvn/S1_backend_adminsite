import React from "react";
import Pagination from "./Pagination";
import moment from "moment-timezone";

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "SiteName" },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId"],
      },
    },
    { dataColumn: "creator", columnTitle: "Creator"},
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
    { dataColumn: "updateOperator", columnTitle: "LastUpdateUser"},
  ];
};

export default function Result (tpDisableUser) {
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
    let bodyRows = tpDisableUser.Datas.map((data, idx) => {
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
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Pagination
        Data={{
          totalPages: tpDisableUser.PageInfo.totalPages,
          pageNumber: tpDisableUser.PageInfo.pageNumber,
          rowCount: tpDisableUser.PageInfo.rowCount,
        }}
        ChangePage={tpDisableUser.ChangePage}
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

const dateTimeFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};