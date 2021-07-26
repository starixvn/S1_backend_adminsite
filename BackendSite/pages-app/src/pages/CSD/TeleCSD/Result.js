import React from "react";
import Pagination from "./Pagination";

const dataColumnDefs = () => {
  return [
    {
      dataColumn: 'userName', columnTitle: 'UserName',
      body: {
          format: userNameFormat,
          subData: ['custId', 'transId']
      }
    },
    { dataColumn: "accountName", columnTitle: "Full name" },
    { dataColumn: "actStatus", columnTitle: "Deposit Status" },
    { dataColumn: "regTime", columnTitle: "Reg Data(Adjusted to GMT+8)" },
    { dataColumn: "phoneNo", columnTitle: "Phone number" },
    { dataColumn: "callOutStatus", columnTitle: "Remark(Adjusted to GMT+8)" },
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
      return element.dataColumn !== "callOutStatus" &&
        element.dataColumn !== "actStatus" ? (
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
      ) : element.dataColumn === "actStatus" ? (
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
            : data[element.dataColumn] === 1
            ? "Non deposit"
            : "Deposited"}
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
            : data[element.dataColumn] !== 0 ?(data['updateRemark'].map((x,idx)=>{
              return <div key={idx}>{x}</div>
            })):null}
          {data[element.dataColumn] === 3 || !data['canUpdate']? null : (
            <div>
              <button
                className="btn btn-search"
                type="submit"
                onClick={props.Complete.bind(this, data)}
              >
                Complete once
              </button>
            </div>
          )}
        </td>
      );
    });

    return <tr key={`result-body-${idx}`}>{row}</tr>;
  });
  return (
    <div className="table">
      {window.subFuncKeys &&
      window.subFuncKeys.includes("tele_export_excel") ? (
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
