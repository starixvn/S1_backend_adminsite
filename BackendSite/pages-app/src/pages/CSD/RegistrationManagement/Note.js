import React from "react";
import moment from "moment-timezone";
import DateTimePicker from "react-datetime";

const dataColumnDefs = () => {
  return [
    { dataColumn: "note", columnTitle: "Note" },
    { dataColumn: "operator", columnTitle: "Operator" },
    {
      dataColumn: "updateTime",
      columnTitle: "UpdateTime",
      body: {
        format: dateFormat,
      },
    },
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

  let bodyRows = props.NoteDatas.map((data, idx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      return element.dataColumn === "note" ? (
        props.EditNoteData.Id !== data["id"] ? (
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
          <td>
            <EditInputField
              defaultValue=""
              dataColumn="EditNote"
              onChange={props.EditNewNote}
            />
          </td>
        )
      ) : element.dataColumn === "action" ? (
        props.EditNoteData.Id !== data["id"] ? (
          <td>
            <div className="button-action">
              {window.subFuncKeys &&
              window.subFuncKeys.includes("rm_edit_note") ? (
                <button
                  className="btn btn-edit"
                  type="submit"
                  onClick={props.EditButton.bind(this, data["id"])}
                >
                  <i className="icon-edit"></i>
                </button>
              ) : null}
              {window.subFuncKeys &&
              window.subFuncKeys.includes("rm_delete_note") ? (
                <button
                  className="btn btn-delete"
                  type="submit"
                  onClick={props.DeleteButton.bind(this, data)}
                >
                  <i className="icon-delete"></i>
                </button>
              ) : null}
            </div>
          </td>
        ) : (
          <td>
            <div className="button-action">
              <button
                className="btn btn-save"
                type="submit"
                onClick={props.SaveButton.bind(this)}
              >
                <i className="icon-save"></i>
              </button>
              <button
                className="btn btn-cancel"
                type="submit"
                onClick={props.CancelButton.bind(this)}
              >
                <i className="icon-cancel"></i>
              </button>
            </div>
          </td>
        )
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
    <div>
      {window.subFuncKeys && window.subFuncKeys.includes("rm_add_note") ? (
        <div className="filter">
          <div className="row-flex">
            <InputField
              defaultValue=""
              dataColumn="AddNote"
              onChange={props.AddNote}
              Size={800}
            />
          </div>
          <div className="row-flex">
            <div className="button-group">
              <button
                className="btn btn-search"
                type="submit"
                onClick={props.SubmitAddNote}
              >
                Add
              </button>
            </div>
          </div>
        </div>
      ) : null}
      <div className="table">
        <table className="table table-striped">
          <thead>
            <tr>{headerRows}</tr>
          </thead>
          <tbody>{bodyRows}</tbody>
        </table>
      </div>
      <div className="button-group">
        <button className="btn btn-search" type="submit" onClick={props.Back}>
          Back
        </button>
      </div>
    </div>
  );
};
const InputField = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <div className="field">
        <input
          type="text"
          defaultValue={props.defaultValue}
          onChange={onChange}
          placeholder="Add a note..."
          style={{ width: `${props.Size}px` }}
        />
      </div>
    </div>
  );
};
const EditInputField = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <div className="field">
        <input
          type="text"
          defaultValue={props.defaultValue}
          onChange={onChange}
        />
      </div>
    </div>
  );
};
const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};
export default Panel;
