import React, { useState, useRef, useEffect } from "react";

const Panel = (props) => {
  let result = props.Data.map((item) => {
    return (
      <div key={`AppType-${item.appType}`} className="panel mt-20">
        <a
          className="panel-title"
          data-toggle="collapse"
          href={`#collapseExample-${item.appType}`}
          role="button"
          aria-expanded="true"
          aria-controls="collapseExample"
        >
          {`${item.siteName}-${item.appType === 1 ? "Andriod" : "IOS"}`}
        </a>
        <Detail Data={item} />
      </div>
    );
  });
  return <div>{result}</div>;
};

const defProtocolType = {
  1: "HTTP",
  2: "HTTPS",
};

const Detail = (props) => {
  const [DetailData, setDetailData] = useState(null);
  const [isEdit, setEdit] = useState(false);
  const [editValue, setEditValue] = useState({
    Domain: "",
    ProtocolType: 0,
  });
  let row = "";

  useEffect(() => {
    setDetailData({
      SiteId: props.Data.siteId,
      Domain: props.Data.domain,
      AppType: props.Data.appType,
      ProtocolType: props.Data.protocolType,
    });
    setEditValue({
      Domain: props.Data.domain,
      ProtocolType: props.Data.protocolType,
    });
  }, []);
  const DetailDataOnchange = (v, dataColumn) => {
    setEditValue({
      ...editValue,
      [dataColumn]: v,
    });
  };

  const EditSubmit = () => {
    let Data={
        SiteId:DetailData.SiteId,
        AppType:DetailData.AppType,
        Domain:editValue.Domain,
        ProtocolType:editValue.ProtocolType
    }
    api
      .post("/api/AppLinkTable/SetAppLink", Data)
      .then(function (response) {
        if (response.data.errorCode === 0) {
        //   props.ClickSave(newDomainList);
          alert(response.data.message);
        } else {
          alert(response.data.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const EditCancel = () => {
    setEdit(false);
    setEditValue(null);
  };
  const EditClick = () => {
    setEdit(true);
  };
  let List =
    DetailData !== null ? (
      <tr>
        <td>
          <DropDownBox
            columnTitle="ProtocolType"
            options={Object.keys(defProtocolType).map((key) => {
              return { value: key, displayText: defProtocolType[key] };
            })}
            hasAll={false}
            selectedValue={DetailData.ProtocolType}
            dataColumn="ProtocolType"
            onChange={DetailDataOnchange}
            isEdit={isEdit}
          />
        </td>
        <td>
          <InputBox
            isEdit={isEdit}
            defaultValue={DetailData.Domain}
            onChange={DetailDataOnchange}
            dataColumn="Domain"
          />
        </td>
        <td>
          {isEdit ? (
            <div className="button-action">
              <button className="btn btn-save" onClick={EditSubmit}>
                <i className="icon-save"></i>
              </button>
              <button className="btn btn-icon-cancel" onClick={EditCancel}>
                <i className="icon-cancel"></i>
              </button>
            </div>
          ) : (
            <div className="button-action">
              <button className="btn btn-edit" onClick={EditClick}>
                <i className="icon-edit"></i>
              </button>
            </div>
          )}
        </td>
      </tr>
    ) : null;
  row = (
    <table className="table table-striped">
      <thead>
        <tr>
          <th scope="col">ProtocolType</th>
          <th scope="col">Domain</th>
          <th scope="col">Action</th>
        </tr>
      </thead>
      <tbody>{List}</tbody>
    </table>
  );
  return (
    <div className="panel-body">
      <div
        className="collapse show"
        id={`collapseExample-${props.Data.appType}`}
      >
        <div className="table">{row}</div>
        {/* {isOpen ? (
          <PopBoxContentAdd
            ClickCancel={CreateCancel}
            SiteId={props.siteId}
            CurrencyId={props.currId}
            ClickSave={handleAdd}
            ReGetData={ReGetData}
          />
        ) : null} */}
      </div>
    </div>
  );
};

const InputBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  return props.isEdit ? (
    <input type="text" defaultValue={props.defaultValue} onChange={onChange} />
  ) : (
    props.defaultValue
  );
};
const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
        {item.displayText}
      </option>
    );
  });
  return props.isEdit ? (
    <div className="input-item">
      <div className="field">
        <select defaultValue={props.selectedValue} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
        ://
      </div>
    </div>
  ) : props.selectedValue === 1 ? (
    "HTTP://"
  ) : (
    "HTTPS://"
  );
};
export default Panel;
