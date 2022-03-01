import React from "react";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";
import moment from "moment";

const TransferTypeFormat = (type) => {
  if (type == 1) {
    return "Deposit";
  }
  if (type == 2) {
    return "Withdrawal";
  }
  if (type == 3) {
    return "Transfer";
  }
  if (type == 4) {
    return "Other";
  }
};

const numberFormat = (num) => {
  return number_format(number_toFixed(num));
};

const ChannelTypeFormat = (type) => {
  if (type == 1) {
    return "Local Bank";
  }
  if (type == 2) {
    return "Head Office";
  }
  if (type == 3) {
    return "PSP";
  }
};

const dataColumnDefs = (props) => {
  return [
    {
      columnTitle: "TransDate",
      dataColumn: "transDate",
      body: {
        attr: {
          class: "text-center",
        },
        format: dateFormat,
      },
    },
    {
      columnTitle: "TranferId",
      dataColumn: "transferId",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "TransferType",
      dataColumn: "transferType",
      body: {
        attr: {
          class: "text-right",
        },
        format: TransferTypeFormat,
      },
    },
    {
      columnTitle: "Channel",
      dataColumn: "channel",
      body: {
        attr: {
          class: "text-right",
        },
        format: ChannelTypeFormat,
      },
    },
    {
      columnTitle: "Agent",
      dataColumn: "agent",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "Pay",
      dataColumn: "payName",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "Site",
      dataColumn: "siteName",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "Currency",
      dataColumn: "isoCode",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "BankCode",
      dataColumn: "bankCode",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "Account No.",
      dataColumn: "accountNo",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "Display Name",
      dataColumn: "accountDisplayName",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "Amount",
      dataColumn: "amount",
      body: {
        attr: {
          class: "text-right",
        },
        format: numberFormat,
      },
    },
    {
      columnTitle: "Fee",
      dataColumn: "fee",
      body: {
        attr: {
          class: "text-right",
        },
        format: numberFormat,
      },
    },
    {
      columnTitle: "Reason",
      dataColumn: "description",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "CreateTime",
      dataColumn: "createTime",
      body: {
        attr: {
          class: "text-right",
        },
        format: dateFormat,
      },
    },
    {
      columnTitle: "Creator",
      dataColumn: "creatorName",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
    {
      columnTitle: "UpdateTime",
      dataColumn: "updateTime",
      body: {
        attr: {
          class: "text-right",
        },
        format: dateFormat,
      },
    },
    {
      columnTitle: "Updator",
      dataColumn: "updatorName",
      body: {
        attr: {
          class: "text-right",
        },
      },
    },
  ];
};

const Panel = (props) => {
  const columnDefs = dataColumnDefs(props);

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
        style={{ position: "sticky", top: "0" }}
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
        <div style={{}}>
          <span style={{ fontWeight: "bold", textAlign: "left" }}>
            PlusNum : {numberFormat(props.PlusNum)} , MinusNum :{" "}
            {numberFormat(props.MinusNum)} , FeeNum :{" "}
            {numberFormat(props.FeeNum)}, Total :
            {numberFormat(props.PlusNum + props.MinusNum + props.FeeNum)}
          </span>
        </div>
        <button
          type="button"
          className="btn btn-approved"
          onClick={props.ExportBtnDisabled ? null : props.ExportToExcel}
          disabled={props.ExportBtnDisabled}
        >
          Export Excel
        </button>
      </div>
      <table className="table table-striped" style={{ width: "auto" }}>
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
    </div>
  );
};

const dateFormat = (createTime) => {
  return moment(createTime).utc(0).format("YYYY-MM-DD HH:mm:ss");
};

export default Panel;
