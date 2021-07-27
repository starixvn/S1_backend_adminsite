import React from "react";
import Pagination from "./Pagination";
import moment from "moment";

const dataColumnDefs = () => {
  return [
    { dataColumn: "menu", columnTitle: "Menu" },
    { dataColumn: "actionType", columnTitle: "ActionType" },
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "operator", columnTitle: "Operator" },
    { dataColumn: "action", columnTitle: "Action" },
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
      return element.dataColumn === "action" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.SetDetailData.bind(this, data)}
            >
              Detail
            </button>
          </div>
        </td>
      ) : element.dataColumn === "actionType" ? (
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
            : data[element.dataColumn] === 1 ? "UPDATE/CREATE":"DELETE"}
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
export default Panel;
