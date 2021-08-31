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
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId", "transId"],
      },
    },
    {
      dataColumn: "currentVipName",
      columnTitle: "Current Vip Level",
    },
    {
      dataColumn: "downgradeVipName",
      columnTitle: "Downgrade Vip Level",
    },
    {
      dataColumn: "isoCode",
      columnTitle: "Currency",
    },
    {
      dataColumn: "depositAmount",
      columnTitle: "Deposit Amount",
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
      dataColumn: "thresholdDeposit",
      columnTitle: "Threshold Deposit",
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
      dataColumn: "turnOverAmount",
      columnTitle: "TurnOver Amount",
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
      dataColumn: "thresholdTurnOver",
      columnTitle: "Threshold TurnOver",
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
      dataColumn: "calculateTime",
      columnTitle: "Calculate Time" + " (" + moment().format("Z") + ")",
      body: {
        format: dateFormat,
      },
    },
    {
      dataColumn: "settleTime",
      columnTitle: "Settle Time" + " (" + moment().format("Z") + ")",
      body: {
        format: settletimeFormat,
      },
    },
    {
      dataColumn: "qualifiedTime",
      columnTitle: "Lastest Qualified Time" + " (" + moment().format("Z") + ")",
      body: {
        format: qualifiedTimeFormat,
      },
    },
    {
      dataColumn: "lastUpdateUser",
      columnTitle: "Last Updator",
    },
    {
      dataColumn: "status",
      columnTitle: "Status",
      body: {
        format: statusFormat,
      },
    },
    {
      dataColumn: "action",
      columnTitle: "Action",
    },
  ];
};
const Panel = (props) => {
  const columnDefs = dataColumnDefs();
  let headerRows = [];
  let canUpdateStatus = props.Datas.find((x) => x.canUpdateStatus == true);
  let permission_Export =
    window.subFuncKeys &&
    window.subFuncKeys.includes("viplevelreview_export_excel");
  let permission_Update =
    window.subFuncKeys && window.subFuncKeys.includes("viplevelreview_update");

  columnDefs.forEach((element, idx) => {
    headerRows.push(
      canUpdateStatus &&
        permission_Update &&
        element.dataColumn === "action" ? (
        <th key={`th-${element.dataColumn}-${idx}`} scope="col">
          {element.columnTitle}
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.AllApproved}
          >
            All Approved
          </button>
        </th>
      ) : (
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
      )
    );
  });

  let bodyRows = props.Datas.map((data, idx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      let Currentvipbg = levelBgcolerFormat(data["currentVipLevel"]);
      let Downgradevipbg = levelBgcolerFormat(data["downgradeVipLevel"]);
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }

      if (
        element.dataColumn === "currentVipName" ||
        element.dataColumn === "downgradeVipName"
      ) {
        return (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
            bgcolor={
              element.dataColumn === "currentVipName"
                ? Currentvipbg
                : Downgradevipbg
            }
          >
            {element.body && element.body.format
              ? element.body.format(data[element.dataColumn], subData)
              : data[element.dataColumn]}
          </td>
        );
      } else if (
        data["canUpdateStatus"] &&
        permission_Update &&
        element.dataColumn === "action"
      ) {
        if (data["status"] === 1 || data["status"] === 3) {
          return (
            <td key={`td-${element.dataColumn}-${idx}`}>
              <div>
                <button
                  className="btn btn-info"
                  type="submit"
                  onClick={props.Approved.bind(this, data)}
                >
                  Approved
                </button>
              </div>
            </td>
          );
        } else if (data["status"] === 2) {
          return (
            <td key={`td-${element.dataColumn}-${idx}`}>
              <div>
                <button
                  className="btn btn-danger"
                  type="submit"
                  onClick={props.Reject.bind(this, data)}
                >
                  Rejected
                </button>
              </div>
            </td>
          );
        } else {
          return <td key={`td-${element.dataColumn}-${idx}`}></td>;
        }
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

    return <tr key={`result-body-${idx}`}>{row}</tr>;
  });

  return (
    <div className="table">
      <div className="table-options">
        {permission_Export ? (
          <button
            type="button"
            className="btn btn-approved"
            onClick={props.ExportToExcel}
          >
            Export Excel
          </button>
        ) : null}
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
const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};
const settletimeFormat = (createTime) => {
  if (createTime === "") {
    return "";
  }
  return moment(createTime).format("l LTS");
};
const qualifiedTimeFormat = (dateTime) => {
  if (dateTime === "") {
    return "";
  }
  return moment(dateTime).format("l LTS");
};
const levelBgcolerFormat = (level) => {
  switch (level) {
    case 1:
      return "c6842c";
    case 2:
      return "c0c0c0";
    case 3:
      return "ffd700";
    case 4:
      return "66CDAA";
    case 5:
      return "4169E1";
    case 6:
      return "B22222";
  }
};
const statusFormat = (status) => {
  switch (status) {
    case 1:
      return "Pending";
    case 2:
      return "Approved";
    case 3:
      return "Rejected";
    case 4:
      return "Settled";
    default:
      return "";
  }
};

export default Panel;
