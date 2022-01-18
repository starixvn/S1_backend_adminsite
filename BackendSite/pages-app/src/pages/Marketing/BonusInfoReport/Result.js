import React from "react";
import Pagination from "./Pagination";
import moment from "moment";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "SiteName" },
    { dataColumn: "transId", columnTitle: "TransId" },
    {
      dataColumn: "promotionName",
      columnTitle: "PromotionType",
    },
    { dataColumn: "bonusCode", columnTitle: "BonusCode" },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId", "transId"],
      },
    },
    { dataColumn: "currencyName", columnTitle: "Currency" },
    {
      dataColumn: "bonus",
      columnTitle: "Bonus",
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
      dataColumn: "maxBonus",
      columnTitle: "MaxBonus",
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
      dataColumn: "rollOver",
      columnTitle: "RollOver",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      dataColumn: "lockedAmount",
      columnTitle: "LockedAmount",
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
      dataColumn: "bonusAmount",
      columnTitle: "BonusAmount",
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
      dataColumn: "cancelBonusAmount",
      columnTitle: "CancelBonusAmount",
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
      dataColumn: "expiryTime",
      columnTitle: "ExpiryTime",
      body: {
        format: dateFormat,
      },
    },
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime",
      body: {
        format: dateFormat,
      },
    },
    {
      dataColumn: "statusName",
      columnTitle: "Status",
    },
    {
      dataColumn: "expiredCancel",
      columnTitle: "ExpiredCancel",
      body: {
        format: boolFormat,
      },
    },
    {
        dataColumn: 'operator',
        columnTitle: 'Operator'
    }   
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
          currencyAmount: props.PageInfo.currencyAmount,
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

const boolFormat = (bool) => {
  return bool ? "TRUE" : "FALSE";
};

export default Panel;
