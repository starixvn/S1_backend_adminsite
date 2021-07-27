const Filter = (props) => {
  let tagListOptions = "";
  if (props.TagList != null) {
    tagListOptions = props.TagList.map((i) => {
      return {
        value: Number(i.id),
        displayText: i.tagName,
      };
    });
  }
  return (
    <div className="filter">
      <div className="row-flex">
        {tagListOptions ? (
          <CheckBoxElem
            Title="Tag"
            Options={tagListOptions}
            Selected={props.FilterData.SelectTag}
            OnChange={props.onChangeCheckBox}
            OnChangeAll={props.onChangeAllCheckBox}
            dataColumn="SelectTag"
          />
        ) : null}
        {subFuncKeys.includes('ca_setCustTag') ? (
        <div className="button-group">
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.ClickSubmit}
          >
            Add
          </button>
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.RemoveTag}
          >
            Remove Tag
          </button>
        </div>
        ) : null}
      </div>
    </div>
  );
};

const CheckBoxElem = ({
  Title,
  Options,
  Selected = [],
  OnChange,
  OnChangeAll,
  dataColumn,
  mainKey,
}) => {
  let opt = Options.map((o) => {
    return (
      <CheckBox
        key={`${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        Options={null}
        onChange={OnChange}
        Title={o.displayText}
        checked={Selected.includes(o.value)}
        Value={o.value}
        dataColumn={dataColumn}
      />
    );
  });
  return (
    <div className="input-item" style={{ width: "auto" }}>
      <label className="label">{Title}</label>
      <CheckBox
        onChange={OnChangeAll}
        Title="All"
        Options={Options}
        checked={Selected.length === Options.length && Selected.length > 0}
        Value="All"
        dataColumn={dataColumn}
      />
      {opt}
    </div>
  );
};

const CheckBox = (props) => {
  const OnChange = (e) => {
    props.onChange(props.Value, props.dataColumn, props.Options);
  };
  return (
    <label className="checkbox checkbox-checked">
      <input
        type="checkbox"
        style={{ width: "0" }}
        value={props.Value}
        checked={props.checked}
        onChange={OnChange}
      />
      <i className="icon icon-checkbox"></i>
      <span className="text">{props.Title}</span>
    </label>
  );
};

export default Filter;
