import React from "react";

const defCategoryType = {
  0: "All",
  1: "Jackpot",
  2: "New",
  3: "Feature",
  4: "Favourite",
};

const Panel = (props) => {
  let categoryRows = Object.keys(defCategoryType).map((_data) => {
    return (
      <div
        key={`CategoryOpt-${defCategoryType[_data]}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`CategoryId_${_data}`}
          name="CategoryList"
          className="custom-control-input"
          value={_data}
          onChange={props.SetCategory.bind(null, _data)}
          checked={_data == props.StateFilter.Category}
        />
        <label className="custom-control-label" htmlFor={`CategoryId_${_data}`}>
          {defCategoryType[_data]}
        </label>
      </div>
    );
  });
  return (
    <div className="filter">
      <label className="col-form-label d-block">Category</label>
      {categoryRows}
    </div>
  );
};
export default Panel;
