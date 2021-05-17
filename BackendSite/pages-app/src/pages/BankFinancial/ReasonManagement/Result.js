import React from "react";
import Pagination from "./Pagination";
import moment from 'moment';

const dataColumnDefs = () => {
  return [
    { dataColumn: "funcKey", columnTitle: "FuncKey" },
    { dataColumn: "reasonId", columnTitle: "ReasonId" },
    { dataColumn: "description", columnTitle: "Description" },
    { dataColumn: "updateTime", columnTitle: "UpdateTime" },
    { dataColumn: "updator", columnTitle: "Updator" },
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
  console.log(props.Datas);
  let bodyRows = props.Datas.map((data, idx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      return element.dataColumn !== "action" ? (
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
      ) : (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Edit.bind(this, data)}
            >
              Edit
            </button>
          </div>
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
const dateFormat = (createTime)=>{
  return moment(createTime).format('l LTS');
}
export default Panel;
