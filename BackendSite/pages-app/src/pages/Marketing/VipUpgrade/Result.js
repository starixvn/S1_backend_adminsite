import React from "react";
import Pagination from "./Pagination";

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "SiteName" },
    {
      dataColumn: 'userName', columnTitle: 'UserName',
      body: {
          format: userNameFormat,
          subData: ['custId', 'transId']
      }
    },
    { dataColumn: "currentVipName", columnTitle: "CurrentVipLevel" },
    { dataColumn: "upgradeVipName", columnTitle: "UpgradeVipLevel" },
    { dataColumn: "currency", columnTitle: "Currency" },
    { dataColumn: "depositAmount", columnTitle: "Deposit Amount" },
    { dataColumn: "thresholdAmount", columnTitle: "Threshold Amount" },
    { dataColumn: "upgradeBonus", columnTitle: "Upgrade Bonus" },
    { dataColumn: "calculateTime", columnTitle: "Calculate Time" },
    { dataColumn: "lastUpdateUser", columnTitle: "LastUpdateUser" },
    { dataColumn: "status", columnTitle: "Status" },
    { dataColumn: "upgrade", columnTitle: "Upgrade" },
  ];
};
const Panel = (props) => {
  const columnDefs = dataColumnDefs();
  let headerRows = [];
  columnDefs.forEach((element, idx) => {
    headerRows.push(
      element.dataColumn !== "upgrade" ? (
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
      ) : props.CanUpgrade &&
        window.subFuncKeys &&
        window.subFuncKeys.includes("vipupgrade_Upgrade") ? (
        <th key={`th-${element.dataColumn}-${idx}`} scope="col">
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.AllUpgrade}
          >
            All Upgrade
          </button>
        </th>
      ) : null
    );
  });

  let bodyRows = props.Datas.map((data, idx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      let Currentvipbg =
        data["currentVipLevel"] === 1
          ? "c6842c"
          : data["currentVipLevel"] === 2
          ? "c0c0c0"
          : data["currentVipLevel"] === 3
          ? "ffd700"
          : data["currentVipLevel"] === 4
          ? "66CDAA"
          : data["currentVipLevel"] === 5
          ? "4169E1"
          : data["currentVipLevel"] === 6
          ? "B22222"
          : null;
      let Upgradevipbg =
        data["upgradeVipLevel"] === 1
          ? "c6842c"
          : data["upgradeVipLevel"] === 2
          ? "c0c0c0"
          : data["upgradeVipLevel"] === 3
          ? "ffd700"
          : data["upgradeVipLevel"] === 4
          ? "66CDAA"
          : data["upgradeVipLevel"] === 5
          ? "4169E1"
          : data["upgradeVipLevel"] === 6
          ? "B22222"
          : null;
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      return element.dataColumn !== "upgrade" ? (
        element.dataColumn === "status" ? (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
          >
            {" "}
            {data[element.dataColumn] === 1
              ? "Pending"
              : data[element.dataColumn] === 2
              ? "Approved"
              : "Rejected"}
          </td>
        ) : element.dataColumn === "currentVipName" ? (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
            bgcolor={Currentvipbg}
          >
            {element.body && element.body.format
              ? element.body.format(data[element.dataColumn], subData)
              : data[element.dataColumn]}
          </td>
        ) : element.dataColumn === "upgradeVipName" ? (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
            bgcolor={Upgradevipbg}
          >
            {element.body && element.body.format
              ? element.body.format(data[element.dataColumn], subData)
              : data[element.dataColumn]}
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
        )
      ) : (data["status"] === 1 || data["status"] === 3) &&
        props.CanUpgrade &&
        window.subFuncKeys &&
        window.subFuncKeys.includes("vipupgrade_Upgrade") ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Upgrade.bind(this, data)}
            >
              Upgrade
            </button>
          </div>
        </td>
      ) : props.CanUpgrade &&
        window.subFuncKeys &&
        window.subFuncKeys.includes("vipupgrade_Upgrade") ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Rdject.bind(this, data)}
            >
              Rdject
            </button>
          </div>
        </td>
      ) : null;
    });

    return <tr key={`result-body-${idx}`}>{row}</tr>;
  });

  return (
    <div className="table">
      {window.subFuncKeys &&
      window.subFuncKeys.includes("vipupgrade_export_excel") ? (
        <div className="table-options">
          <button
            type="button"
            className="btn btn-approved"
            onClick={props.ExportToExcel}
          >
            Export Excel
          </button>
        </div>
      ) : null}
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
  return <a href={"/CustManage/CustInfo?UserName=" + userName + "&custId=" + subData.custId + "&TransId=" + subData.transId} target="_blank">{userName}</a>
}

export default Panel;
