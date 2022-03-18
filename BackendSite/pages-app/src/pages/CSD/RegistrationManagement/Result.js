import React from "react";
import moment from "moment-timezone";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";
import Pagination from "./Pagination";

const defStatusType = {
  1: "Pending ",
  2: "Done ",
  3: "Invalid ",
};

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "Site" },
    { dataColumn: "currency", columnTitle: "Currency" },
    { dataColumn: "affCode", columnTitle: "Affiliates Code" },
    { dataColumn: "agent", columnTitle: "Agent" },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId"],
      },
    },
    { dataColumn: "accountName", columnTitle: "Full Name" },
    { dataColumn: "phoneNo", columnTitle: "Phone No." },
    { dataColumn: "phoneVerified", columnTitle: "Verify" },
    {
      dataColumn: "regTime",
      columnTitle: "RegTime/Date GMT+8",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "actStatus", columnTitle: "Deposit Staus" },
    { dataColumn: "action", columnTitle: "ACTION" },
    { dataColumn: "status", columnTitle: "STATUS" },
    { dataColumn: "note", columnTitle: "NOTE" },
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
      return element.dataColumn === "phoneVerified" ? (
        <td
          key={`td-${element.dataColumn}-${idx}`}
          className={
            element.body && element.body.attr && element.body.attr.class
              ? element.body.attr.class
              : ""
          }
          bgcolor={data[element.dataColumn] === 1 ? "#E8FFE8" : "#b80007"}
        >
          {data[element.dataColumn] === 1 ? "Verified" : "NO"}
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
          {data[element.dataColumn] === 1 ? "Non deposit" : "Deposited"}
        </td>
      ) : element.dataColumn === "action" ? (
        <td
          key={`td-${element.dataColumn}-${idx}`}
          bgcolor={data["status"] === 1 ? "#b80007" : "#bdbdbd"}
        >
          {"Call"}
        </td>
      ) : element.dataColumn === "status" ? (
        props.Editstatus.CustId === data["custId"] ? (
          <td
            key={`td-${element.dataColumn}-${idx}`}
            className={
              element.body && element.body.attr && element.body.attr.class
                ? element.body.attr.class
                : ""
            }
          >
            <DropDownBox
              options={Object.keys(defStatusType).map((key) => {
                return { value: key, displayText: defStatusType[key] };
              })}
              hasAll={false}
              selectedValue={data[element.dataColumn]}
              dataColumn="Status"
              onChange={props.UpdateStatus}
              custId={data["custId"]}
              siteId={data["siteId"]}
              // CancelChangeStatus={props.CancelChangeStatus}
            />
            {/* <div className="button-action">
              <button
                className="btn btn-cancel"
                type="submit"
                onClick={props.CancelChangeStatus.bind(this)}
              >
                <i className="icon-cancel"></i>
              </button>
            </div> */}
          </td>
        ) : (
          <td key={`td-${element.dataColumn}-${idx}`}>
            <div>
              <div className="button-action">
                {data["status"] === 1
                  ? "Pending"
                  : data["status"] === 2
                  ? "Done"
                  : "Invalid"}
                <button
                  className="btn btn-edit"
                  type="submit"
                  onClick={props.ChangeStatus.bind(this, data["custId"])}
                >
                  <i className="icon-edit"></i>
                </button>
              </div>
            </div>
          </td>
        )
      ) : element.dataColumn === "note" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div className="button-action">
            <button
              className="btn btn-edit"
              type="submit"
              onClick={props.EditNote.bind(this, data["custId"])}
            >
              <i className="icon-edit"></i>
            </button>
          </div>
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
      <div className="table-options">
        {props.Isloading === true ? (
          <div>
            <button
              type="button"
              className="btn btn-refresh"
              disabled={true}
              onClick={null}
            >
              Loading
            </button>
          </div>
        ) : (
          <div>
            <button
              type="button"
              className="btn btn-refresh"
              onClick={props.Refresh}
            >
              Refresh({props.Count})
            </button>
          </div>
        )}
        <button
          type="button"
          className="btn btn-approved"
          onClick={props.Result_ExportToExcel}
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
        "&TransId="
      }
      target="_blank"
    >
      {userName}
    </a>
  );
};
const numberFormat = (number) => {
  return number_format(number_toFixed(number, 2));
};
const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};

const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.custId, props.siteId);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${item.value}`} value={item.value}>
        {item.displayText}
      </option>
    );
  });
  return (
    <div className="input-item">
      <tr>
        <td>
          <div className="field">
            <select
              defaultValue={props.selectedValue}
              value={props.selectedValue}
              onChange={onChange}
            >
              {props.hasAll ? <option value="0">All</option> : null}
              {optionRows}
            </select>
          </div>
        </td>
        {/* <td>
          <div className="button-action">
            <button
              className="btn btn-cancel"
              type="submit"
              onClick={props.CancelChangeStatus.bind(this)}
            >
              <i className="icon-cancel"></i>
            </button>
          </div>
        </td> */}
      </tr>
    </div>
  );
};
export default Panel;
