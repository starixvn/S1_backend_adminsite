import React from "react";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const statusFormat = (type) => {
  if (type == 1) {
    return "Initial";
  }
  if (type == 2) {
    return "Open";
  }
  if (type == 3) {
    return "Suspend";
  }
  if (type == 4) {
    return "Closed";
  }
};

const purposeFormat = (type) => {
  let result = type.split(",");
  let purposeString = "";

  if (result.find((x) => x == 1)) {
    purposeString = purposeString + "Deposit,";
  }
  if (result.find((x) => x == 2)) {
    purposeString = purposeString + "Withdrawal,";
  }
  if (result.find((x) => x == 3)) {
    purposeString = purposeString + "Transfer,";
  }
  return purposeString.substr(0, purposeString.length - 1);
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
      columnTitle: "Account No.",
      dataColumn: "accountNo",
      body: {
        attr: {
          class: "text-center",
        },
      },
    },
    {
      columnTitle: "Account Name",
      dataColumn: "accountName",
      body: {
        attr: {
          class: "text-center",
        },
      },
    },
    {
      columnTitle: "Channel",
      dataColumn: "channel",
      body: {
        attr: {
          class: "text-center",
        },
        format: ChannelTypeFormat,
      },
    },
    {
      columnTitle: "Agent",
      dataColumn: "agent",
      body: {
        attr: {
          class: "text-center",
        },
      },
    },
    {
      columnTitle: "Pay",
      dataColumn: "payName",
      body: {
        attr: {
          class: "text-center",
        },
      },
    },
    {
      columnTitle: "Purpose",
      dataColumn: "purpose",
      body: {
        attr: {
          class: "text-center",
        },
        format: purposeFormat,
      },
    },
    {
      columnTitle: "Status",
      dataColumn: "accountStatus",
      body: {
        attr: {
          class: "text-center",
        },
        format: statusFormat,
      },
    },
    {
      columnTitle: "Site",
      dataColumn: "siteName",
      body: {
        attr: {
          class: "text-center",
        },
      },
    },
    {
      columnTitle: "Currency",
      dataColumn: "isoCode",
      body: {
        attr: {
          class: "text-center",
        },
      },
    },
    {
      columnTitle: "Deposit Amount",
      dataColumn: "depositAmount",
      body: {
        attr: {
          class: "text-center",
        },
        format: numberFormat,
      },
    },
    {
      columnTitle: "Withdrawal Amount",
      dataColumn: "withdrawalAmount",
      body: {
        attr: {
          class: "text-center",
        },
        format: numberFormat,
      },
    },
    {
      columnTitle: "Transfer Amount",
      dataColumn: "transferAmount",
      body: {
        attr: {
          class: "text-center",
        },
        format: numberFormat,
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

export default Panel;
