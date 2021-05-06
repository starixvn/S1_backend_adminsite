import React from "react";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    { dataColumn: "reportName", columnTitle: "" },
    {
      dataColumn: "noPlayer",
      columnTitle: "NoPlayer",
      header: {
        attr: {
          class: "text-right",
        },
      },
      body: {
        attr: {
          class: "text-right",
        },
        format: numberFormatInteger,
      },
    },
    {
      dataColumn: "noAdjustmentPlayers",
      columnTitle: "NoAdjustmentPlayers",
      header: {
        attr: {
          class: "text-right",
        },
      },
      body: {
        attr: {
          class: "text-right",
        },
        format: numberFormatInteger,
      },
    },
    {
      dataColumn: "noAdjustmentAmount",
      columnTitle: "NoAdjustmentAmount",
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
      dataColumn: "totalDeposit",
      columnTitle: "TotalDeposit",
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
      dataColumn: "totalWithdrawal",
      columnTitle: "TotalWithdrawal",
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
      dataColumn: "totalBonus",
      columnTitle: "TotalBonus",
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
      dataColumn: "vaildTurnover",
      columnTitle: "VaildTurnover",
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
      dataColumn: "ggr",
      columnTitle: "GGR",
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
      dataColumn: "margin",
      columnTitle: "Margin",
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
      dataColumn: "netRevenue",
      columnTitle: "NetRevenue",
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
      dataColumn: "netCash",
      columnTitle: "NetCash",
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

  let bodyRows = props.DashBoardData.todayData.map((data, idx) => {
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
    <div>
      <table>
        <thead>
          <tr>
            <td>
              <div
                style={{
                  border: "2px #ccc solid",
                  borderRadius: "5px",
                  width: "200px",
                  backgroundColor: "#00A300",
                }}
              >
                <div style={{ textAlign: "center" }}>
                  Online Players {props.DashBoardData.onlinePlayers}
                </div>
              </div>
            </td>
            <td>
              <div
                style={{
                  border: "2px #ccc solid",
                  borderRadius: "5px",
                  width: "200px",
                  backgroundColor: "#6E6EFF",
                }}
              >
                <div style={{ textAlign: "center" }}>
                  Today New Reg. Players{" "}
                  {props.DashBoardData.todayNewRegPlayers}
                </div>
              </div>
            </td>
            <td>
              <div
                style={{
                  border: "2px #ccc solid",
                  borderRadius: "5px",
                  width: "200px",
                  backgroundColor: "#FFFF82",
                }}
              >
                <div style={{ textAlign: "center" }}>
                  Total Reg. Players {props.DashBoardData.totalRegPlayers}
                </div>
              </div>
            </td>
            <td>
              <div
                style={{
                  border: "2px #ccc solid",
                  borderRadius: "5px",
                  width: "200px",
                  backgroundColor: "#A3A3FF",
                }}
              >
                <div style={{ textAlign: "center" }}>
                  Pending Deposit {props.DashBoardData.pendingDeposit}
                </div>
              </div>
            </td>
            <td>
              <div
                style={{
                  border: "2px #ccc solid",
                  borderRadius: "5px",
                  width: "200px",
                  backgroundColor: "#FFAC12",
                }}
              >
                <div style={{ textAlign: "center" }}>
                  Pending Withdrawal {props.DashBoardData.pendingWithdrawal}
                </div>
              </div>
            </td>
          </tr>
        </thead>
      </table>
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
    </div>
  );
};
const numberFormat = (number) => {
  return number_format(number_toFixed(number, 2));
};
const numberFormatInteger = (number) => {
    return number_format(number_toFixed(number, 0));
  };

export default Panel;
