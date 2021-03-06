import React from 'react';
import PropTypes from 'prop-types';
import cn from 'classnames';

const getClassName = (isActive) => cn({
    slider: true,
    active: isActive,
});


class Range extends React.Component {
    state = { value: 1 }

    componentDidMount() {
        const { onChange } = this.props;

        this.setState({ value: 0 });

        if (onChange) {
            onChange(Number(0));
        }
    }

    handleSliderChange = ({ target: { value } }) => {
        const { onChange } = this.props;
        this.setState({ value });

        if (onChange) {
            onChange(Number(value));
        }
    }

    render() {
        const { value } = this.state;

        return (
            <input
                type="range"
                max="1"
                value={value}
                className={getClassName(Number(value))}
                onChange={(e) => this.handleSliderChange(e)}
            />
        );
    }
}

Range.propTypes = { onChange: PropTypes.func.isRequired };

export default Range;
