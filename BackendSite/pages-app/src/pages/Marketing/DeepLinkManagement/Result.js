import React, { useState, useRef, useEffect } from "react";
import { api } from "../../../module/WebAPI";
const Panel = (props) => {
  let result = props.Data.map((item) => {
    return (
      <div key={`AppType-${item.type}-${item.siteId}`} className="panel mt-20">
        <a
          className="panel-title"
          data-toggle="collapse"
          href={`#collapseExample-${item.type}`}
          role="button"
          aria-expanded="true"
          aria-controls="collapseExample"
        >
          {`${
            item.type === 1 ? "Android" : item.type === 2 ? "IOS" : "Desktop"
          }`}
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
  console.log(props);
  const [DetailData, setDetailData] = useState(null);
  const [isEdit, setEdit] = useState(false);
  const [editValue, setEditValue] = useState({
    Domain: "",
    ProtocolType: 1,
  });
  let row = "";

  useEffect(() => {
    setDetailData({
      SiteId: props.Data.siteId,
      Domain: props.Data.domain,
      Type: props.Data.type,
      ProtocolType: props.Data.protocolType,
      DeepLinkId: props.Data.deepLinkId,
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
    let Data = {
      SiteId: parseInt(DetailData.SiteId),
      Type: parseInt(DetailData.Type),
      Domain: editValue.Domain,
      ProtocolType: parseInt(editValue.ProtocolType),
      DeepLinkId: parseInt(DetailData.DeepLinkId),
    };
    api
      .post("/api/DeepLinkManagement/SetDeepLink", Data)
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setDetailData({
            ...DetailData,
            Domain: Data.Domain,
            ProtocolType: Data.ProtocolType,
          });
          setEdit(false);
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
    setEditValue({
      Domain: DetailData.Domain,
      ProtocolType: DetailData.ProtocolType,
    });
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
          {isEdit ? null : <DetailDomain Domain={DetailData.Domain} />}
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
      <div className="collapse show" id={`collapseExample-${props.Data.type}`}>
        <div className="table">{row}</div>
      </div>
    </div>
  );
};
const DetailDomain = (props) => {
  const [Domain, setDomain] = useState("");
  useEffect(() => {
    setDomain(props.Domain);
  }, [props]);

  return <div>{Domain}</div>;
};
const InputBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  return props.isEdit ? (
    <input type="text" defaultValue={props.defaultValue} onChange={onChange} />
  ) : null;
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
