import React from "react";

const Panel = (props) => {
  let row = "";
  if (props.Datas === null) {
    row = (
      <div className="table-options">
        <button className="btn btn-add" onClick={props.Add}>
          Add
        </button>
      </div>
    );
  } else {
    let temppassword =
      props.opt.password === null ? props.Datas.password : props.opt.password;
    row = (
      <table className="table table-striped">
        <thead>
          <tr>
            <th scope="col">Password</th>
            <th scope="col">Action</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <InputBox
                isEdit={props.isEdit}
                defaultValue={temppassword}
                onChange={props.passwordEdit}
              />
            </td>
            <td>
              {props.isEdit ? (
                <div className="button-action">
                  <button className="btn btn-save" onClick={props.EditSubmit}>
                    <i className="icon-save"></i>
                  </button>
                  <button className="btn btn-icon-cancel" onClick={props.EditCancel}>
                    <i className="icon-cancel"></i>
                  </button>
                </div>
              ) : (
                <div className="button-action">
                  <button className="btn btn-edit" onClick={props.EditClick}>
                    <i className="icon-edit"></i>
                  </button>
                </div>
              )}
            </td>
          </tr>
        </tbody>
      </table>
    );
  }
  return (
    <div key={`Site-${props.opt.siteId}`} className="panel mt-20">
      <div className="panel-body">
        <div
          className="collapse show"
          id={`collapseExample-${props.opt.siteId}`}
        >
          <div className="table">{row}</div>
          {props.isOpen ? (
            <div>
              <div>
                <h1>Add Password</h1>
                <div className="field">
                    <label>Password</label>
                    <input
                      defaultValue={props.opt.password}
                      onChange={(e) => {
                        props.setOpt({
                          ...props.opt,
                          password: e.target.value,
                        });
                      }}
                    />
                  </div>
                  <button
                    onClick={props.AddSubmit}
                  >
                    Save
                  </button>
                  <button
                    onClick={props.Cancel}
                  >
                    Cancel
                  </button>
                  <div className="clearfix"></div>
              </div>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
};

const InputBox = (props) => {
  return props.isEdit ? (
    <input
      defaultValue={props.defaultValue}
      onChange={(e) => {
        let value = e.target.value;
        props.onChange(value);
      }}
    />
  ) : (
    props.defaultValue
  );
};

export default Panel;
