import React from "react";
import Pagination from "./Pagination";
import moment from "moment-timezone";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "SiteName" },
    { dataColumn: "title", columnTitle: "Title" },
    { dataColumn: "content", columnTitle: "Content" },
    { dataColumn: "author", columnTitle: "LastUpdateUser" },
    {
      dataColumn: "updateTime",
      columnTitle: "LastUpdateTime (Adjusted to GMT+8)",
      body: { format: dateFormat },
    },
    {
      dataColumn: "edit",
      columnTitle: "Edit",
      body: { SpecialItem: "actionBtn" },
    },
    {
      dataColumn: "result",
      columnTitle: "Process Result",
    },
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
      let body_content = null;
      if (element.body && element.body.SpecialItem === "actionBtn") {
        body_content = (
          <ActionButtons
            clickEdit={props.ClickEdit.bind(null, data.batchId)}
            clickDelete={props.ClickDelete.bind(null, data.batchId)}
          />
        );
      } else if (element.dataColumn === "result") {
        body_content = (
          <ExportButtons
            downloadResult={props.DownloadResult.bind(null, data.batchId)}
          />
        );
      } else {
        body_content =
          element.body && element.body.format
            ? element.body.format(data[element.dataColumn])
            : data[element.dataColumn];
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
          {body_content}
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
          totalPages: props.PageInfo.totalPages,
          pageNumber: props.PageInfo.pageNumber,
          rowCount: props.PageInfo.rowCount,
        }}
        ChangePage={props.ChangePage}
      />
    </div>
  );
};

const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};

const numberFormat = (number) => {
  return number_format(number_toFixed(number, 2));
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

const ActionButtons = (props) => {
  return (
    <>
      <div className="button-action" onClick={props.clickEdit}>
        <button className="btn btn-edit">
          <i className="icon-edit"></i>
        </button>
      </div>
      <div className="button-action" onClick={props.clickDelete}>
        <button className="btn btn-icon-delete">
          <i className="fa fa-trash-alt"></i>
        </button>
      </div>
    </>
  );
};

const ExportButtons = (props) => {
  return (
    <>
      <div>
        <button
          type="button"
          className="btn btn-approved"
          onClick={props.downloadResult}
        >
          Export Result
        </button>
      </div>
    </>
  );
};

export default Panel;
